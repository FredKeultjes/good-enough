/*
Arduino program by Fred Keultjes

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

*/

#include <Arduino.h>
#include <SPI.h>
#include <TFT_eSPI.h> // Graphics and font library for ST7735 driver chip
#include <MD_AD9833.h>
#include <Bounce2.h>
#include <EEPROM.h>

/*

AD9833 VCC    to ESP32 3.3V
       OGND   to       GND
       SDATA  to       23 (VSPI MOSI)
       SCLK   to       18 (VSPI SCK)
       FSYNC  to       5  (VSPI SS)
       AGND   to output GND
       OUT    to output signal

TFT    LED    to       collector of PNP transistor, base to pin 17
       SCK    to ESP32 14 (HSPI SCK)
       SDA    to       13 (HSPI MOSI)
       AO     to       0  (DC)
       RESET  to       2  (RST)
       CS     to       15  (HSPI CS)
       GND    to       GND
       VCC    to       3.3V



rotation encoder:
       D0/CLK to ESP32  33
       D1/DT  to        32
       +      to        3.3V
       GND    to        GND
       
buttons:
       STEP   to        16
       WAVE   to        12
       SWEEP  to        27
       FREQ   to        22
       PHASE  to        26
       ROTENC to ESP32  25

input: 
       FREQ MODULATION to ESP32   4
       PHASE MODULATION to ESP32  19
*/


TFT_eSPI tft = TFT_eSPI();  // Invoke library, pins defined in User_Setup.h


// Pins for SPI comm with the AD9833 IC
#define DATA  23	///< SPI Data pin number
#define CLK   18	///< SPI Clock pin number
#define FSYNC 5	///< SPI Load pin number (FSYNC in AD9833 usage)

MD_AD9833	AD(FSYNC); // Hardware SPI (VSPI)
//MD_AD9833	AD(DATA, CLK, FSYNC); // Arbitrary SPI pins

#define ROTENC_D0     33
#define ROTENC_D1     32
#define BUTTON_CYCLE_ROTENC  25
#define BUTTON_CYCLE_FREQ 22
#define BUTTON_CYCLE_PHASE 26
#define BUTTON_CYCLE_WAVE 12
#define BUTTON_CYCLE_SWEEP 27
#define BUTTON_CYCLE_STEP 16
#define PIN_TFT_LED 17
#define PIN_HALFVBAT 35

#define PIN_INPUT_FREQMODULATION 4
#define PIN_INPUT_PHASEMODULATION 19

#define NUM_BUTTONS 6
const uint8_t BUTTON_PINS[NUM_BUTTONS] = {BUTTON_CYCLE_ROTENC, BUTTON_CYCLE_FREQ, BUTTON_CYCLE_PHASE, BUTTON_CYCLE_WAVE, BUTTON_CYCLE_SWEEP, BUTTON_CYCLE_STEP};

#define BUTTONINDEX_CYCLE_ROTENC  0
#define BUTTONINDEX_CYCLE_FREQ 1
#define BUTTONINDEX_CYCLE_PHASE 2
#define BUTTONINDEX_CYCLE_WAVE 3
#define BUTTONINDEX_CYCLE_SWEEP 4
#define BUTTONINDEX_CYCLE_STEP 5

#define TFT_BACKGROUND TFT_BLACK


#define EEPROM_BASE_ADDRESS 128
#define EEPROM_MAGIC_BYTE1 83
#define SAVE_SETTINGS_AFTER_MS 1500

#define DIM_BACKLIGHT_AFTER_MS 10000

#define FREQ_MIN 1
#define FREQ_MAX 9999999
#define FREQ_DEFAULT 1000



enum rotEncInpEnum_t
{
  ROTENC_FREQ0,
  ROTENC_FREQ1,
  ROTENC_PHASE0,
  ROTENC_PHASE1,
  ROTENC_SWEEPMIN,
  ROTENC_SWEEPMAX,
  ROTENC_SWEEPSPEED,
  ROTENC_MAX = ROTENC_SWEEPSPEED
};

unsigned long  g_dimBacklightAfter = millis() + LONG_MAX;
uint8_t g_backLight = HIGH;

void SwitchOnBacklightWithTimeout()
{
  g_dimBacklightAfter = millis() + DIM_BACKLIGHT_AFTER_MS;
  if( g_backLight != LOW )
  {
    pinMode(PIN_TFT_LED, OUTPUT);
    digitalWrite(PIN_TFT_LED, LOW);
    g_backLight = LOW;
  }
}
void SwitchOffBacklight()
{
  if( g_backLight != HIGH )
  {
    pinMode(PIN_TFT_LED, OUTPUT);
    digitalWrite(PIN_TFT_LED, HIGH);
    g_backLight = HIGH;
  }
  g_dimBacklightAfter = millis() + LONG_MAX;
}
void LoopDimBacklight()
{
  if( ((long)millis()-(long)g_dimBacklightAfter)>0 )
  {
    SwitchOffBacklight();
  }
}

#pragma pack(1)
class CAD9833Settings
{
public:
  uint32_t freq0;  // THIS MEMBER MUST BE FIRST OF PERSISTENT DATA
  uint32_t freq1;
  uint8_t freqChannel; // 0=MD_AD9833::CHAN_0, 1 = MD_AD9833::CHAN_1, 2=using modulation input;
  uint32_t freqSweepMin;
  uint32_t freqSweepMax;
  int32_t freqSweepStep;
  uint32_t freqSweep;
  uint16_t sweepDelayInMs;
  uint16_t sweepTimeInMs;
  uint16_t phaseTenths0;
  uint16_t phaseTenths1;
  uint8_t phaseChannel; // 0=MD_AD9833::CHAN_0, 1 = MD_AD9833::CHAN_1, 2=using modulation input;
  MD_AD9833::mode_t waveMode;
  bool sweepOn;
  uint32_t freqStep;
  rotEncInpEnum_t rotEncInpSelect;

  char  afterAllData; // THIS MEMBER MUST BE LAST OF PERSISTENT DATA
  unsigned long  saveAfter;

  CAD9833Settings();
  bool Save();
  bool Read();
  inline void SetDirty()
  {
    saveAfter = millis() + SAVE_SETTINGS_AFTER_MS;
    SwitchOnBacklightWithTimeout();
  }
  inline void ResetDirty()
  {
    saveAfter = millis() + LONG_MAX;
  }
  inline void loop()
  {
    if( ((long)millis()-(long)saveAfter)>0 )
    {
       Save();
    }
  }
} settings;
#pragma pack()

#define EEPROM_END  (EEPROM_BASE_ADDRESS+sizeof(CAD9833Settings)+1)


CAD9833Settings::CAD9833Settings()
{
  freq0 = FREQ_DEFAULT;
  freq1 = FREQ_DEFAULT;
  freqChannel = 0; // 0=MD_AD9833::CHAN_0, 1 = MD_AD9833::CHAN_1, 2=using modulation input;
  freqSweepMin = FREQ_DEFAULT;
  freqSweepMax = FREQ_DEFAULT+2000;
  freqSweepStep = 5;
  freqSweep = FREQ_DEFAULT;
  sweepDelayInMs = 1;
  sweepTimeInMs = 5000;
  phaseTenths0 = 0;
  phaseTenths1 = 900;
  phaseChannel = 0; // 0=MD_AD9833::CHAN_0, 1 = MD_AD9833::CHAN_1, 2=using modulation input;
  waveMode = MD_AD9833::MODE_SINE;
  sweepOn = false;
  freqStep = 100;
  rotEncInpSelect = ROTENC_FREQ0;
  afterAllData = 0;

  ResetDirty();
}

bool CAD9833Settings::Save()
{
  Serial.printf("[save]");
  if( !EEPROM.begin(EEPROM_END) )
  {
      Serial.printf("CAD9833Settings::Save EEPROM.begin failed\r\n");
      return false;
  }

  EEPROM.writeByte(EEPROM_BASE_ADDRESS, EEPROM_MAGIC_BYTE1);

  size_t nToWrite = &afterAllData-(const char*)&freq0;
  if( EEPROM.writeBytes(EEPROM_BASE_ADDRESS+1, &freq0, nToWrite) != nToWrite )
  {
    Serial.printf("CAD9833Settings::Write failed\r\n");
    return false;       
  }
  if( !EEPROM.commit())
  {
    Serial.printf("CAD9833Settings::Write commit failed\r\n");
    return false;    
  }
  ResetDirty();
  return true;
}

bool CAD9833Settings::Read()
{
  if( !EEPROM.begin(EEPROM_END) )
  {
      Serial.printf("CAD9833Settings::Read EEPROM.begin failed\r\n");
      return false;
  }
  if( EEPROM.readByte(EEPROM_BASE_ADDRESS) != EEPROM_MAGIC_BYTE1 )
  {
      Serial.printf("CAD9833Settings::Read found to data\r\n");
      return false;      
  }

  size_t nToRead = &afterAllData-(const char*)&freq0;
  if( EEPROM.readBytes(EEPROM_BASE_ADDRESS+1, &freq0, nToRead) != nToRead )
  {
      Serial.printf("CAD9833Settings::Read failed\r\n");
      return false; 
  }
  ResetDirty();
  return true;
}



void initTft()
{
  tft.init();
  tft.setRotation(1);
  tft.fillScreen(TFT_BACKGROUND);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);  // Adding a black background colour erases previous text automatically

  tft.setTextColor(TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.setTextDatum( TL_DATUM );
  tft.drawString("Step", 0, 71, 1);
  tft.drawString("Wave", 135, 71, 1);
  tft.drawString("-", 95, 119, 1);
  tft.drawString("VBAT", 94, 71, 1);
  tft.setTextDatum( BL_DATUM );
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.drawString("Hz", 100, 28, 1);
  tft.drawString("Hz", 100, 64, 1);
  SwitchOnBacklightWithTimeout();
}

void writeScreenFrequency0()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.freq0);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TR_DATUM );
  auto txtWidth = tft.drawString(szBuf, 97, 9, 4);
  tft.fillRect(0, 10, 97-txtWidth, 24,TFT_BACKGROUND);

  if(!settings.sweepOn )
  {
    AD.setFrequency(MD_AD9833::CHAN_0, settings.freq0);
  }
}
void writeScreenFrequency1()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.freq1);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TR_DATUM );
  auto txtWidth = tft.drawString(szBuf, 97, 45, 4);
  tft.fillRect(0, 46, 97-txtWidth, 24,TFT_BACKGROUND);

  AD.setFrequency(MD_AD9833::CHAN_1, settings.freq1);
}
void writeScreenActiveFrequency()
{
  if( settings.sweepOn )
  {
    tft.setTextColor(TFT_LIGHTGREY, TFT_BACKGROUND);
    tft.setTextDatum( TL_DATUM );
    tft.drawString("Frequency 1", 0, 0, 1);
    tft.drawString("Frequency 2", 0, 35, 1);  
    AD.setActiveFrequency(MD_AD9833::CHAN_0);
  }
  else
  {
    if( settings.freqChannel==0)
    {
      tft.setTextColor(TFT_YELLOW, TFT_BACKGROUND);
      tft.setTextDatum( TL_DATUM );
      tft.drawString("Frequency 1", 0, 0, 1);
      tft.setTextColor(TFT_LIGHTGREY, TFT_BACKGROUND);
      tft.drawString("Frequency 2", 0, 35, 1);  
    }
    else if( settings.freqChannel==1)
    {
      tft.setTextColor(TFT_LIGHTGREY, TFT_BACKGROUND);
      tft.setTextDatum( TL_DATUM );
      tft.drawString("Frequency 1", 0, 0, 1);
      tft.setTextColor(TFT_YELLOW, TFT_BACKGROUND);
      tft.drawString("Frequency 2", 0, 35, 1);  
    }
    else
    {
      tft.setTextColor(TFT_YELLOW, TFT_BACKGROUND);
      tft.setTextDatum( TL_DATUM );
      tft.drawString("Frequency 1", 0, 0, 1);
      tft.drawString("Frequency 2", 0, 35, 1);  
    }
    AD.setActiveFrequency(settings.freqChannel==0 ? MD_AD9833::CHAN_0 : settings.freqChannel==1 ? MD_AD9833::CHAN_1 : (MD_AD9833::channel_t)digitalRead(PIN_INPUT_FREQMODULATION));
  }
}
void writeScreenFreqStep()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.freqStep);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TR_DATUM );
  auto txtWidth = tft.drawString(szBuf, 48, 80, 2);
  tft.fillRect(0, 80, 48-txtWidth, 16,TFT_BACKGROUND);
}
void writeScreenPhase0()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.phaseTenths0/10);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TR_DATUM );
  auto txtWidth = tft.drawString(szBuf, 159, 9, 4);
  tft.fillRect(118, 10, 41-txtWidth, 24,TFT_BACKGROUND);

  AD.setPhase(MD_AD9833::CHAN_0, settings.phaseTenths0);
}
void writeScreenPhase1()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.phaseTenths1/10);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TR_DATUM );
  auto txtWidth = tft.drawString(szBuf, 159, 45, 4);
  tft.fillRect(118, 46, 41-txtWidth, 24,TFT_BACKGROUND);

  AD.setPhase(MD_AD9833::CHAN_1, settings.phaseTenths1);  
}

void writeScreenActivePhase()
{
  if( settings.phaseChannel==0)
  {
    tft.setTextColor(TFT_YELLOW, TFT_BACKGROUND);
    tft.setTextDatum( TL_DATUM );
    tft.drawString("Phase 1", 119, 0, 1);
    tft.setTextColor(TFT_LIGHTGREY, TFT_BACKGROUND);
    tft.drawString("Phase 2", 119, 35, 1);  
  }
  else if( settings.phaseChannel==1 )
  {
    tft.setTextColor(TFT_LIGHTGREY, TFT_BACKGROUND);
    tft.setTextDatum( TL_DATUM );
    tft.drawString("Phase 1", 119, 0, 1);
    tft.setTextColor(TFT_YELLOW, TFT_BACKGROUND);
    tft.drawString("Phase 2", 119, 35, 1);  
  }
  else
  {
     tft.setTextColor(TFT_YELLOW, TFT_BACKGROUND);
    tft.setTextDatum( TL_DATUM );
    tft.drawString("Phase 1", 119, 0, 1);
    tft.drawString("Phase 2", 119, 35, 1);  
  }
  AD.setActivePhase(settings.phaseChannel==0 ? MD_AD9833::CHAN_0 : settings.phaseChannel==1 ? MD_AD9833::CHAN_1 : (MD_AD9833::channel_t)digitalRead(PIN_INPUT_PHASEMODULATION));
}
void writeScreenWaveMode()
{
   const char * szP;
   switch(settings.waveMode)
   {
     case MD_AD9833::MODE_OFF: szP = "OFF"; break;
     case MD_AD9833::MODE_SINE: szP = "SINE"; break;
     case MD_AD9833::MODE_TRIANGLE: szP = "TRI"; break;
     case MD_AD9833::MODE_SQUARE1: szP = "SQ1"; break;
     case MD_AD9833::MODE_SQUARE2: szP = "SQ2"; break;
     default: szP= "?"; break;
   }
   tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
   tft.setTextDatum( TL_DATUM );
   auto txtWidth = tft.drawString(szP, 134, 80, 2);
   tft.fillRect(134+txtWidth, 80, 30-txtWidth, 16,TFT_BACKGROUND);

   AD.setMode(settings.waveMode);
}
void updateSweepStep()
{
  settings.freqSweep = settings.freqSweepMin;
  settings.sweepDelayInMs = 10;  
  settings.freqSweepStep = (settings.freqSweepMax < settings.freqSweepMin ? settings.freqSweepMin-settings.freqSweepMax : settings.freqSweepMax-settings.freqSweepMin) / (settings.sweepTimeInMs / settings.sweepDelayInMs);
  if(settings.freqSweepStep==0)
      settings.freqSweepStep = 1;
  Serial.printf("[debug] freqSweepStep=%d\n", settings.freqSweepStep);
  settings.SetDirty();
}
void writeScreenSweepMin()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.freqSweepMin);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TR_DATUM );
  auto txtWidth = tft.drawString(szBuf, 92, 115, 2);
  tft.fillRect(37, 115, 55-txtWidth, 16,TFT_BACKGROUND);
  updateSweepStep();
}
void writeScreenSweepMax()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.freqSweepMax);
  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TL_DATUM );
  auto txtWidth = tft.drawString(szBuf, 102, 115, 2);
  tft.fillRect(102+txtWidth, 115, 50-txtWidth, 16,TFT_BACKGROUND);
  updateSweepStep();
}
void writeScreenSweepTime()
{
  char szBuf[20];
  sprintf(szBuf, "%d", settings.sweepTimeInMs/1000);

  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TL_DATUM );
  auto txtWidth = tft.drawString(szBuf, 0, 120, 1);
  tft.fillRect(0+txtWidth, 120, 25-txtWidth, 8,TFT_BACKGROUND);
  updateSweepStep();
}
void writeScreenSweepOn()
{
   tft.setTextColor(settings.sweepOn ? TFT_YELLOW : TFT_LIGHTGREY, TFT_BACKGROUND);
   tft.setTextDatum( TL_DATUM );
   tft.drawString("Sweep", 0, 110, 1);

   if( settings.sweepOn )
   {
       AD.setActiveFrequency(MD_AD9833::CHAN_0);
       Serial.printf("[debug] chan0\r");
   }
}
void writeScreenRotEncMode()
{
  tft.setTextFont(1);
  tft.setTextDatum( TL_DATUM );
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_FREQ0 ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("Fq1", 0, 100);
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_FREQ1 ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("Fq2", 20, 100);
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_PHASE0 ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("Ph1", 40, 100);
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_PHASE1 ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("Ph2", 60, 100);
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_SWEEPMIN ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("SwMn", 80, 100);
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_SWEEPMAX ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("SwMx", 105, 100);
  tft.setTextColor(settings.rotEncInpSelect==ROTENC_SWEEPSPEED ? TFT_GREEN : TFT_LIGHTGREY, TFT_BACKGROUND);
  tft.drawString("SwSp", 130, 100);
}
void writeVBAT()
{
  char szBuf[10];
  //NOTE: lolin32 lite has no internal VBAT divider.
  sprintf(szBuf, "%1.2fv", (float) analogRead(PIN_HALFVBAT) /581 /*=manually adjusted*/ );

  tft.setTextColor(TFT_GREEN, TFT_BACKGROUND);
  tft.setTextDatum( TL_DATUM );
  tft.drawString(szBuf, 94, 80, 2);
}
unsigned long vbatWritten = 0;
void loopVBAT()
{
  if( ((long)millis()-(long)vbatWritten)>0 )
  {
      writeVBAT();
      vbatWritten = millis()+1000;
  }
}

void rotEncApply(bool up)
{
  switch(settings.rotEncInpSelect)
  {
    case ROTENC_FREQ0:
      if( up )
      {
        if( settings.freq0+settings.freqStep <= FREQ_MAX)
        {
          settings.freq0 += settings.freqStep;
          settings.SetDirty();
          writeScreenFrequency0();
        }
      }
      else
      {
        if( settings.freq0 >= FREQ_MIN + settings.freqStep)
        {
          settings.freq0 -= settings.freqStep;
          settings.SetDirty();
          writeScreenFrequency0();
        }
      }
      break;
    case ROTENC_FREQ1:
      if( up )
      {
        if( settings.freq1+settings.freqStep <= FREQ_MAX)
        {
          settings.freq1 += settings.freqStep;
          settings.SetDirty();
          writeScreenFrequency1();
        }
      }
      else
      {
        if( settings.freq1 >= FREQ_MIN + settings.freqStep)
        {
          settings.freq1 -= settings.freqStep;
          settings.SetDirty();
          writeScreenFrequency1();
        }
      }
      break;
    case ROTENC_PHASE0:
      if( up )
      {
        if( settings.phaseTenths0>=(3600-50) )
        {
          settings.phaseTenths0 -= (3600-50);
        }
        else
        {
          settings.phaseTenths0 += 50;
        }
      }
      else
      {
        if( settings.phaseTenths0<50 )
        {
          settings.phaseTenths0 += (3600-50);
        }
        else
        {
          settings.phaseTenths0 -= 50;
        }
      }
      settings.SetDirty();
      writeScreenPhase0();
      break;
    case ROTENC_PHASE1:
      if( up )
      {
        settings.phaseTenths1 += 50;
        if( settings.phaseTenths1>=3600)
          settings.phaseTenths1 -= 3600;
      }
      else
      {
        settings.phaseTenths1 -= 50;
        if( settings.phaseTenths1<0)
          settings.phaseTenths1 += 3600;
      }
      writeScreenPhase1();
      settings.SetDirty();
      break;      
    case ROTENC_SWEEPMIN:
      if( up )
      {
        if( settings.freqSweepMin+settings.freqStep <= FREQ_MAX)
        {
          settings.freqSweepMin += settings.freqStep;
          writeScreenSweepMin();
          settings.SetDirty();
        }
      }
      else
      {
        if( settings.freqSweepMin >= FREQ_MIN + settings.freqStep)
        {
          settings.freqSweepMin -= settings.freqStep;
          writeScreenSweepMin();
          settings.SetDirty();
        }
      }
      break;
    case ROTENC_SWEEPMAX:
      if( up )
      {
        if( settings.freqSweepMax+settings.freqStep <= FREQ_MAX)
        {
          settings.freqSweepMax += settings.freqStep;
          writeScreenSweepMax();
          settings.SetDirty();
        }
      }
      else
      {
        if( settings.freqSweepMax >= FREQ_MIN + settings.freqStep)
        {
          settings.freqSweepMax -= settings.freqStep;
          writeScreenSweepMax();
          settings.SetDirty();
        }
      }
      break;
    case ROTENC_SWEEPSPEED:
      if( up )
      {
        if( settings.sweepTimeInMs+1000 <= 20000 )
        {
          settings.sweepTimeInMs += 1000;
          writeScreenSweepTime();
          settings.SetDirty();
        }
      }
      else
      {
        if( settings.sweepTimeInMs >= 2000 )
        {
          settings.sweepTimeInMs -= 1000;
          writeScreenSweepTime();
          settings.SetDirty();
        }
      }
    default:
      Serial.printf("[debug] rotEncApply unsupported rotEncInpSelect %d\n", settings.rotEncInpSelect);
      break;
  }
}

int rotEncState0 = 0;

void initRotaryEncoder()
{
   pinMode(ROTENC_D0, INPUT_PULLUP);
   pinMode(ROTENC_D1, INPUT_PULLUP);

   rotEncState0 = digitalRead(ROTENC_D0);
}

void loopRotaryEncoder()
{
   int aState = digitalRead(ROTENC_D0); // Reads the "current" state of the outputA
   // If the previous and the current state of the outputA are different, that means a Pulse has occured
   if (aState != rotEncState0){     
     // If the outputB state is different to the outputA state, that means the encoder is rotating clockwise
     if (digitalRead(ROTENC_D1) != aState) { 
       rotEncApply(true);
     } else {
       rotEncApply(false);
     }
     rotEncState0 = aState; // Updates the previous state of the outputA with the current state
   } 
}

Bounce * buttons = new Bounce[NUM_BUTTONS];

void initButtons()
{
  for (int i = 0; i < NUM_BUTTONS; i++) {
    buttons[i].attach( BUTTON_PINS[i] , INPUT_PULLUP  );       //setup the bounce instance for the current button
    buttons[i].interval(25);              // interval in ms
  }
}

void loopButtons() 
{
  buttons[BUTTONINDEX_CYCLE_ROTENC].update();
  if( buttons[BUTTONINDEX_CYCLE_ROTENC].fell() )
  {
    if( settings.rotEncInpSelect>=ROTENC_MAX )
    {
      settings.rotEncInpSelect = rotEncInpEnum_t(0);
    }
    else
    {
      settings.rotEncInpSelect = rotEncInpEnum_t(settings.rotEncInpSelect+1);
    }
    writeScreenRotEncMode();
    settings.SetDirty();
  }

  buttons[BUTTONINDEX_CYCLE_FREQ].update();
  if( buttons[BUTTONINDEX_CYCLE_FREQ].fell() )
  {
    settings.freqChannel = settings.freqChannel == 2 ? 0 : (settings.freqChannel+1);
    writeScreenActiveFrequency();
    settings.SetDirty();
  }

  buttons[BUTTONINDEX_CYCLE_PHASE].update();
  if( buttons[BUTTONINDEX_CYCLE_PHASE].fell() )
  {
    settings.phaseChannel = settings.phaseChannel == 2 ? 0 : (settings.phaseChannel+1);
    writeScreenActivePhase();
    settings.SetDirty();
  }

  buttons[BUTTONINDEX_CYCLE_WAVE].update();
  if( buttons[BUTTONINDEX_CYCLE_WAVE].fell() )
  {
    if( settings.waveMode>=MD_AD9833::MODE_TRIANGLE )
    {
      settings.waveMode = MD_AD9833::mode_t(0);
    }
    else
    {
      settings.waveMode = MD_AD9833::mode_t(settings.waveMode+1);
    }
    writeScreenWaveMode();
    settings.SetDirty();
  }

  buttons[BUTTONINDEX_CYCLE_SWEEP].update();
  if( buttons[BUTTONINDEX_CYCLE_SWEEP].fell() )
  {
    settings.sweepOn = !settings.sweepOn;
    writeScreenSweepOn();
    writeScreenActiveFrequency();
    if( !settings.sweepOn )
    {
      if( settings.freqChannel==1)
      {
        AD.setFrequency(MD_AD9833::CHAN_1, settings.freq1);
      }
      else
      {
        AD.setFrequency(MD_AD9833::CHAN_0, settings.freq0);
      }
    }
    settings.SetDirty();
  }


  buttons[BUTTONINDEX_CYCLE_STEP].update();
  if( buttons[BUTTONINDEX_CYCLE_STEP].fell() )
  {
    switch( settings.freqStep )
    {
      case 1:
        settings.freqStep=10;
        break;
      case 10:
        settings.freqStep=100;
        break;
      case 100:
        settings.freqStep=1000;
        break;
      case 1000:
        settings.freqStep=10000;
        break;
      case 10000:
        settings.freqStep=100000;
        break;
      default:
        settings.freqStep = 1;
        break;
    }
    writeScreenFreqStep();
    settings.SetDirty();
  }
}

void loopSweep()
{
  if( settings.freqSweep + settings.freqSweepStep > (settings.freqSweepMin>settings.freqSweepMax ? settings.freqSweepMin : settings.freqSweepMax) )
  {
    settings.freqSweep = settings.freqSweepMin>settings.freqSweepMax ? settings.freqSweepMax : settings.freqSweepMin;
  }
  else
  {
    settings.freqSweep += settings.freqSweepStep;
  }
//Serial.printf("%ld,", (long)settings.freqSweep);
  AD.setFrequency(MD_AD9833::CHAN_0, settings.freqSweep);
  settings.SetDirty();
  delay(settings.sweepDelayInMs);
}

void onFreqModulationChange()
{
  if(settings.freqChannel==2)
  {
    AD.setActiveFrequency(digitalRead(PIN_INPUT_FREQMODULATION) ? MD_AD9833::CHAN_1 : MD_AD9833::CHAN_0);
  }
}

void onPhaseModulationChange()
{
  if(settings.phaseChannel==2)
  {
    AD.setActivePhase(digitalRead(PIN_INPUT_PHASEMODULATION) ? MD_AD9833::CHAN_1 : MD_AD9833::CHAN_0);
  }
}

void setup()
{
  Serial.begin(115200);

  initRotaryEncoder();
  settings.Read();
  initTft();
  initButtons();
  AD.begin();

  //TODO: read previous settings

  writeScreenFrequency0();
  writeScreenFrequency1();
  writeScreenPhase0();
  writeScreenPhase1();
  writeScreenActivePhase();
  writeScreenWaveMode();
  writeScreenSweepOn();
  writeScreenActiveFrequency();
  writeScreenRotEncMode();
  writeScreenFreqStep();
  writeScreenSweepMin();
  writeScreenSweepMax();
  writeScreenSweepTime();

  pinMode(PIN_INPUT_FREQMODULATION, INPUT_PULLUP);
  attachInterrupt(PIN_INPUT_FREQMODULATION, onFreqModulationChange, CHANGE);
  pinMode(PIN_INPUT_PHASEMODULATION, INPUT_PULLUP);
  attachInterrupt(PIN_INPUT_PHASEMODULATION, onPhaseModulationChange, CHANGE);
}



void loop()
{
  loopRotaryEncoder();
  loopButtons();
  if( settings.sweepOn )
  {
    loopSweep();
  }
  settings.loop();
  loopVBAT();
  LoopDimBacklight();
}


/*
Programming by Fred Keultjes

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Backup2014
{
    public class BackupThread
    {
        private Thread thread = null;

        private Mutex backupJobMutex = new Mutex();
        private JobConfiguration backupJob = null;

        /// <summary>
        /// uiWorkloadMutex locks many variables
        /// </summary>
        private Mutex uiWorkloadMutex = new Mutex();
        #region variables locked by uiWorkloadMutex
        private StringBuilder errorsToShow = new StringBuilder();
        private StringBuilder infoLinesToShow = new StringBuilder();
        private string ConfirmationText = string.Empty;
        private DialogResult ConfirmationResult = DialogResult.Cancel;
        private string lastStatus = string.Empty;
        private long progressCurrent = 0;
        private long progressMax = 0;
        private DateTime progressStartTime = DateTime.MinValue;
        private long progressStartCurrent = 0;
        private long overallProgressCurrent = 0;
        private long overallProgressMax = 0;
        private DateTime overallProgressStartTime = DateTime.MinValue;
        private long overallProgressStartCurrent = 0;
        private int messageboxShowing = 0;
        #endregion

        private AutoResetEvent confirmationResultReady = new AutoResetEvent(false);
        private ManualResetEvent cancelThreadEvent = new ManualResetEvent(false);
        private ManualResetEvent cancelJobEvent = new ManualResetEvent(false);
        private AutoResetEvent jobStartReqEvent = new AutoResetEvent(false);

        public BackupThread()
        {
        }
#region methods calls by main form
        public void Start()
        {
            thread = new Thread(BackupThreadProc);
            thread.Start();
        }


        public void Stop()
        {
            cancelThreadEvent.Set();
        }

        public bool StartJob(JobConfiguration backupJob)
        {
            backupJobMutex.WaitOne();
            if (this.backupJob!=null )
            {
                backupJobMutex.ReleaseMutex();
                return false;
            }
            this.backupJob = backupJob;
            backupJobMutex.ReleaseMutex();
            jobStartReqEvent.Set();
            return true;
        }

        public void StopJob()
        {
            cancelJobEvent.Set();
        }

        /// <summary>
        /// Handle ui actions
        /// </summary>
        /// <param name="txtStatus"></param>
        /// <returns>backuping status</returns>
        public bool PollUIWorkload(MainForm mainForm, TextBox txtStatus, ProgressBar progressBar, TextBox txtProgress, ProgressBar progressBarOverall, TextBox txtProgressOverall, TextBox txtErrors)
        {
            uiWorkloadMutex.WaitOne();
            if( lastStatus.Length>0 )
            {
                txtStatus.Text = lastStatus;
                lastStatus = string.Empty;
            }
            if (progressMax>0)
            {
                if (progressCurrent == -1)
                {
                    progressBar.Visible = false;
                    txtProgress.Visible = true;
                    progressBar.Maximum = Int32.MaxValue;
                    progressBar.Value = 0;
                    txtProgress.Text = string.Format("Scanning.... {0}.",  Utilities.RepresentFileSize(progressMax));
                }
                else
                {
                    progressBar.Visible = true;
                    txtProgress.Visible = true;
                    progressBar.Maximum = Int32.MaxValue;
                    progressBar.Value = progressCurrent < 0 ? 0 : progressCurrent > progressMax ? Int32.MaxValue
                                : (Int32)(((double)progressCurrent * (double)Int32.MaxValue) / progressMax);

                    string estimatedEnd = string.Empty;
                    if (progressStartTime != DateTime.MinValue && progressCurrent > progressStartCurrent)
                    {
                        estimatedEnd = " Estimated end: " + progressStartTime.AddMilliseconds((double)(progressMax - progressStartCurrent) / (double)(progressCurrent - progressStartCurrent) * (DateTime.Now - progressStartTime).TotalMilliseconds).ToString("HH:mm:ss");
                    }
                    txtProgress.Text = string.Format("Backup {0:0}% complete. {1} to transfer.{2}", (double)progressCurrent * 100 / (double)progressMax, Utilities.RepresentFileSize(progressMax - progressCurrent), estimatedEnd);
                }
            }
            else
            {
                progressBar.Visible = false;
                txtProgress.Visible = false;
            }

            if (overallProgressMax > 0)
            {
                long usedOverallProgressCurrent = overallProgressCurrent < 0 ? 0 : overallProgressCurrent + progressCurrent;
                progressBarOverall.Visible = true;
                txtProgressOverall.Visible = true;
                progressBarOverall.Maximum = Int32.MaxValue;
                progressBarOverall.Value = usedOverallProgressCurrent < 0 ? 0 : usedOverallProgressCurrent > overallProgressMax ? Int32.MaxValue
                            : (Int32)(((double)usedOverallProgressCurrent * (double)Int32.MaxValue) / overallProgressMax);

                string estimatedEnd = string.Empty;
                if (overallProgressStartTime != DateTime.MinValue && usedOverallProgressCurrent > overallProgressStartCurrent)
                {
                    estimatedEnd = " Estimated end: " + overallProgressStartTime.AddMilliseconds((double)(overallProgressMax - overallProgressStartCurrent) / (double)(usedOverallProgressCurrent - overallProgressStartCurrent) * (DateTime.Now - overallProgressStartTime).TotalMilliseconds).ToString("HH:mm:ss");
                }
                txtProgressOverall.Text = string.Format("Backup {0:0}% complete. {1} to transfer.{2}", (double)usedOverallProgressCurrent * 100 / (double)overallProgressMax, Utilities.RepresentFileSize(overallProgressMax - usedOverallProgressCurrent), estimatedEnd);
            }
            else
            {
                progressBarOverall.Visible = false;
                txtProgressOverall.Visible = false;
            }

            if (errorsToShow.Length > 0 && messageboxShowing==0)
            {
                string msg = errorsToShow.ToString();
                errorsToShow.Length = 0;

#if false
                int countLF = 0;
                int linePos = 0;
                int i;
                for (i = 0; i < msg.Length; i++)
                {
                    switch(msg[i])
                    {
                        case '\n': 
                            countLF++; 
                            linePos=0; 
                            break;
                        case '\r':
                            linePos = 0;
                            break;
                        default:
                            linePos++;
                            if (linePos > 200)
                            {
                                countLF++;
                                linePos = 0;
                            }
                            break;
                    }
                    if( countLF>=40)
                    {
                        break;
                    }
                }
                if (countLF >= 40)
                {
                    errorsToShow.Append(msg.Substring(i));
                    msg = msg.Substring(0, i);
                }
#endif
                txtErrors.Text += msg;
            }
            if (infoLinesToShow.Length > 0 && messageboxShowing==0)
            {
                string msg = infoLinesToShow.ToString();
                infoLinesToShow.Length = 0;
                messageboxShowing++;
                uiWorkloadMutex.ReleaseMutex();
                mainForm.RestoreWindowForMessage();
                MessageBox.Show(msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                uiWorkloadMutex.WaitOne();
                messageboxShowing--;
            }
            if (ConfirmationText.Length>0)
            {
                string msg = ConfirmationText;
                ConfirmationText = string.Empty;
                messageboxShowing++;
                uiWorkloadMutex.ReleaseMutex();
                mainForm.RestoreWindowForMessage();
                DialogResult result = MessageBox.Show(msg, "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                uiWorkloadMutex.WaitOne();
                messageboxShowing--;
                ConfirmationResult = result;
                ConfirmationText = string.Empty;
                confirmationResultReady.Set();
            }
            bool isBackuping = backupJob !=null;
            uiWorkloadMutex.ReleaseMutex();
            return isBackuping;
        }
        #endregion

        #region methods called by backuptask

        public void PostMessage(bool error, string msg)
        {
            try
            {
                uiWorkloadMutex.WaitOne();
                if (error)
                    errorsToShow.AppendLine(msg);
                else
                    infoLinesToShow.AppendLine(msg);
            }
            finally
            {
                uiWorkloadMutex.ReleaseMutex();
            }
        }
        public void PostStatus(string status)
        {
            uiWorkloadMutex.WaitOne();
            lastStatus = status;
            uiWorkloadMutex.ReleaseMutex();
        }

        public DialogResult Confirm(string msg)
        {
            uiWorkloadMutex.WaitOne();
            if (ConfirmationText.Length > 0)
            {
                uiWorkloadMutex.ReleaseMutex();
                System.Diagnostics.Debug.Assert(false); // kan niet
                return DialogResult.Cancel;
            }
            ConfirmationText = msg;
            uiWorkloadMutex.ReleaseMutex();

            int waitIndex = WaitHandle.WaitAny(new WaitHandle[] { confirmationResultReady, cancelThreadEvent });
            if( waitIndex == 0)
            {
                uiWorkloadMutex.WaitOne();
                DialogResult result = ConfirmationResult;
                uiWorkloadMutex.ReleaseMutex();
                return result;
            }
            return DialogResult.Cancel;
        }

        public void ShowProgress(long max, long current)
        {
            uiWorkloadMutex.WaitOne();
            progressCurrent = current;
            progressMax = max;
            uiWorkloadMutex.ReleaseMutex();
        }

        public void SetProgressStart(long progressStartCurrent)
        {
            uiWorkloadMutex.WaitOne();
            this.progressStartTime = DateTime.Now;
            this.progressStartCurrent = progressStartCurrent;
            uiWorkloadMutex.ReleaseMutex();
        }

        public void ShowOverallProgress(long max, long current)
        {
            uiWorkloadMutex.WaitOne();
            overallProgressCurrent = current;
            overallProgressMax = max;
            uiWorkloadMutex.ReleaseMutex();
        }

        public void SetOverallProgressStart(long overallProgressStartCurrent)
        {
            uiWorkloadMutex.WaitOne();
            this.overallProgressStartTime = DateTime.Now;
            this.overallProgressStartCurrent = overallProgressStartCurrent;
            uiWorkloadMutex.ReleaseMutex();
        }

        public bool IsCancelJobRequested()
        {
            try
            {
                return cancelJobEvent.WaitOne(1);
            }
            catch
            {

            }
            return false;
        }
        public void ResetCancelJobRequest()
        {
            cancelJobEvent.Reset();
        }

        #endregion


        private void BackupThreadProc()
        {
            for (; ; )
            {
                backupJobMutex.WaitOne();
                if( backupJob != null )
                {
                    backupJobMutex.ReleaseMutex();

                    BackupTask task = new BackupTask(backupJob, this);
                    task.BackupAllSources();
                    backupJob = null;
                }
                else
                {
                    backupJobMutex.ReleaseMutex();
                }


                int waitIndex = WaitHandle.WaitAny(new WaitHandle[] { cancelThreadEvent, jobStartReqEvent }, 10000);
                if (waitIndex == 0)
                {
                    cancelThreadEvent.Reset();
                    break;
                }
            }
        }
    }

   
}

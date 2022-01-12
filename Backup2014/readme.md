# Fred's Backup 2014 - Non-destructive backup

This is a very basic backup program. Basically it will just copy from a set of source locations to a target location. With some extra features:
- For each source location you can add some exclusions.
- The program will save its settings in a file `backupjob.xml` in the same folder as the .exe file
- Each time the backup program runs an instance directory is create that will contains logging and the previous versions of updated or deleted files to be archived.
- To save space update or deleted files are archieved in a zip file (except huge files).

For instance, as source specify several folders on your laptop and is target a folder on an external disk or network share.

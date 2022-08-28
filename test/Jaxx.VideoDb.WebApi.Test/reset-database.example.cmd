SET PATH_TO_MYSQL_EXE=C:\Program Files\MySQL\MySQL Workbench 8.0 CE\
SET MYSQL_HOST=your_host_ip
SET MYSQL_USER=your_user
SET MYSQL_PASSWORD=your_password
SET MYSQL_SCHEMA=your_schema
SET IMPORT_FILE=path_to_your_sqldump_file


"%PATH_TO_MYSQL_EXE%mysql.exe" --host=%MYSQL_HOST% --user=%MYSQL_USER% --password=%MYSQL_PASSWORD% < %IMPORT_FILE%

PAUSE
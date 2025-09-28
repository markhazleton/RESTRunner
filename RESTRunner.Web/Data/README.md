# RESTRunner.Web Data Directory

This directory contains all data files for the RESTRunner web application:

## Structure

- **configurations/** - Test configuration JSON files
- **collections/** - Postman collection JSON files  
- **results/** - Execution result CSV/JSON files
- **logs/** - Execution log files

## File Naming Conventions

- Configurations: `{guid}.json`
- Collections: `{name}_{timestamp}.json`
- Results: `execution_{timestamp}_{guid}.csv`
- Logs: `execution_{timestamp}_{guid}.log`

## Notes

- Files are automatically created and managed by the application
- Old files can be cleaned up via the maintenance API
- Backup this directory to preserve configurations and results
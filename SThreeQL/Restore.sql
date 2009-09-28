RESTORE DATABASE @RestoreCatalog 
FROM DISK = @Path
WITH  
	FILE = 1,  
	MOVE @Name TO @RestoreCatalogPath,  
	MOVE @LogName TO @RestoreLogPath,  
	NOUNLOAD,  
	REPLACE,  
	STATS = 10
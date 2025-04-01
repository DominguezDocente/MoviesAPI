/****** Arroja la deistancia segun ubicación data por cada Cinema ******/

Declare @MyLocation GEOGRAPHY = 'POINT (-75.60055511220942 6.267161708608983)'

SELECT TOP (1000) [Id]
      ,[Name]
      ,[Location].ToString()
	  ,[Location].STDistance(@MyLocation) as Distance
  FROM [MoviesDb].[dbo].[CinemaRooms]
  Order By
	[Location].STDistance(@MyLocation)
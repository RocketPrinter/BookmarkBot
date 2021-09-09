#!/bin/bash

sleep 5

 until dotnet ef database update --no-build; do
 	echo "SQL Server is starting up. Waiting 5 seconds..."
 	sleep 5
 done
 
 echo "SQL Server is up - running app"
 exec dotnet run --no-build
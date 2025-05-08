curl -X POST http://localhost:5266/api/auth/login \
-H "Content-Type: application/json" \
-d '{
  "username": "admin",
  "password": "admin123"
}'



winx@DESKTOP-PMVN4J3:~/SRMonitor_grp6$ dotnet ef migrations list --project SRMCore
winx@DESKTOP-PMVN4J3:~/SRMonitor_grp6$ dotnet ef migrations add CreateUsersTable --project SRMCore
dwinx@DESKTOP-PMVN4J3:~/SRMonitor_grp6$ dotnet ef migrations add AddDevicesTable --project SRMCore



docker exec -it <container_id> sh

dotnet build
dotnet ef migrations add InitialCreate --project SRMCore
dotnet ef database update --project SRMCore


docker exec -it postgres psql -U postgres -d srmcore
SELECT * FROM "Users";
SELECT * FROM "Devices";


docker-compose up --build
docker exec -it postgres psql -U postgres -d srmcore







curl -X POST http://localhost:5277/api/auth/generate-token \
-H "Content-Type: application/json" \
-d '{"username": "admin", "role": "admin"}'


sudo docker exec -it redis redis-cli


127.0.0.1:6379> KEYS *
1) "token:admin"
127.0.0.1:6379> GET token:admin
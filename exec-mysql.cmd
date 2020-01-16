rem remove images
docker system prune -a

rem create a net
rem docker network create mynet

rem stop mysql_container
docker stop mysql_container
docker rm mysql_container

rem pull and start mysql_container
docker pull mysql/mysql-server
docker run -p 3307:3306 --name mysql_container -e MYSQL_ROOT_PASSWORD=pass -e MYSQL_DATABASE=mydb --restart always -d mysql/mysql-server:latest

rem stop mysqldriver_container
docker stop mysqldriver_container
docker rm mysqldriver_container

rem build image from sources
docker build -t mysqldriver_image -f dockerfile-build .


rem run test image
docker run --link mysql_container:mysql_container --name mysqldriver_container -e "CONNECTION_STRING=Location=mysql_container;Data Source=todosdb;User ID=user;Password=pass;Port=3307" mysqldriver_image dotnet test ./src/MySqlDriverCs.Core.Tests/MySqlDriverCs.Core.Tests.csproj

rem docker run -it --link mysql_container:mysql_container  --entrypoint=bash --name mysqldriver_container -e "CONNECTION_STRING=Location=mysql_container;Data Source=todosdb;User ID=user;Password=pass;Port=3307" mysqldriver_image
pause

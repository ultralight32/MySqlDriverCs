rem docker-compose -f docker-compose-mysql.yml up --build --abort-on-container-exit
rem docker stop some-mysql
rem docker rm some-mysql
rem docker run --name some-mysql -e MYSQL_ROOT_PASSWORD=pass -e MYSQL_DATABASE=db -e MYSQL_USER=user -e MYSQL_PASSWORD=pass -d mysql:latest
rem docker exec -it some-mysql bash
rem  mysql -hsome-mysql -uuser -p

docker stop mysql-8
docker rm mysql-8
docker pull mysql/mysql-server
docker run -p 3307:3306 --name mysql-8 -e MYSQL_ROOT_PASSWORD=pass -e MYSQL_DATABASE=mydb --restart always -d mysql/mysql-server:latest
docker logs mysql-8
docker exec -it mysql-8 bash
rem  mysql -uroot -ppass mydb
docker build -f dockerfile-custom-mysql -t custom_mysql .
docker run --name ubuntu_bash --rm -i -t custom_mysql bash

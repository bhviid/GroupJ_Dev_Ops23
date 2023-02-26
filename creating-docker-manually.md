### Create Commands

```docker
docker build -t minitwit -f Dockerfile .
docker create --name MiniTwit minitwit
docker run -d --name MiniTwit -p 5235:80 minitwit 
```

### Kill Commands

```docker
docker kill MiniTwit
docker rm MiniTwit
docker image rm minitwit -f
```

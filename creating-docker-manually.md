### Create Commands

```docker
docker build -t minitwit -f MiniTwitPostgres.Dockerfile .
cd remote/db/ | docker-compose up -d
docker run --name MiniTwit -p 5235:80 -p 5432:5432 -e connection_string="Host=localhost;Port=5432;Database=minitwit;Username=ole;Password=veryhardcode" minitwit
```

### Kill Commands

```docker
docker kill MiniTwit
docker rm MiniTwit
docker image rm minitwit -f
```

### Mock Postgres DB

```docker


```

```bash
"Host=localhost;Port=5432;Database=minitwit;Username=ole;Password=veryhardcode"
```

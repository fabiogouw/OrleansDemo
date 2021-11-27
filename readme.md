# Orleans Demo

Este é um exemplo de uso do framework Orleans da Microsoft para a construção de aplicações escaláveis e resilientes.
A ideia aqui é simular um sistema que gerencia dispositivos que medem a temperatura (IoT). Cada um dos dispositivos envia para uma API a sua temperatura atual, que é armazenada nos atores (grains) hospedados na camada intermediária (stateful).

![Alt text](docs/architecture.png)

Este exemplo considera a persistência dos atores e a gestão do cluster com o uso de um banco de dados SQL Server. A URL de conexão deve ser configurada como um secret da aplicação (chaves *ClusterStorage* e *GrainStorage*)

```bash
dotnet user-secrets set "ConnectionStrings:GrainStorage" "Server=172.17.0.2;Database=orleans;User Id=sa;Password=ABC;"

dotnet user-secrets set "ConnectionStrings:ClusterStorage" "Server=172.17.0.2;Database=orleans;User Id=sa;Password=ABC;"
```

O exemplo foi desenvolvido em uma máquina Ubuntu, com .NET 6.0 e rodando um SQL Server 2019 como um container (Docker).

Obs. Os scripts para a criação das tabelas de storage em bases relacionais podem ser encontrados na documentação do Orleans, em [https://dotnet.github.io/orleans/docs/host/configuration_guide/adonet_configuration.html](https://dotnet.github.io/orleans/docs/host/configuration_guide/adonet_configuration.html). Para este exemplo, é necessário executar os scripts SQLServer-Main.sql e SQLServer-Clustering.sql.

## Como executar

Para executar esta aplicação, após a criação da base de dados num servidor SQL Server e da execução dos scripts necessários, basta executar os passos abaixo:

1. Navegar até a pasta src\OrleansDemo.SiloHost e executar o comando ```dotnet run```. Isso irá inicializar o silo, onde os grains serão instanciados (vale testar a criação de vários silos, para simular a comunicação entre processos / hosts).

``` bash
cd src\OrleansDemo.SiloHost
dotnet run
```

2. Navegar até a pasta src\OrleansDemo.WebApp e executar o comando ```dotnet run```. Isso irá iniciar o client da demonstração (aplicação web que recebe as requisições http)

``` bash
cd src\OrleansDemo.WebApp
dotnet run
```

3. Simular um dispositivo IoT enviando dados da sua temperatura (PUT http)

``` bash
curl -X PUT -d value=21.234 https://localhost:5001/api/devices/1
```

4. Obter o dado que foi enviado pelo dispositivo IoT através de um GET

``` bash
curl https://localhost:5001/api/devices/1
```
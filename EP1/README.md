# Para realizar a build dos executáveis, rode um dos seguintes commandos:

## Compilar ambos os executáveis

```
dotnet build -c Servidor_Release --no-incremental; dotnet build -c Cliente_Release --no-incremental
```

### Observação:
O projeto deve ser compilado utilizando .NET 8


## Extra

### Servidor:

#### Modo Debug:
```
dotnet build -c Servidor_Debug
```

#### Modo Release:
```
dotnet build -c Servidor_Release
```

### Cliente:

#### Modo Debug:
```
dotnet build -c Cliente_Debug
```

#### Modo Release:
```
dotnet build -c Cliente_Release
```
# 🚀 Cadastro de Produção CREOI - Automação Web

Sistema de automação para cadastro de produção no sistema CREOI, utilizando Playwright para automação web e Windows Forms para interface gráfica.

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Funcionalidades](#funcionalidades)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Como Usar](#como-usar)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Planilha de Dados](#planilha-de-dados)
- [Suporte](#suporte)
- [Licença](#licença)

## 🎯 Sobre o Projeto

O **Cadastro de Produção CREOI** é uma aplicação desktop que automatiza o processo de cadastro de materiais e serviços no sistema CREOI. A ferramenta lê dados de uma planilha Excel, realiza login automático no sistema, pesquisa DC e sequencial, e cadastra os itens conforme as informações da planilha.

## ⚙️ Funcionalidades

- ✅ **Login Automático** com OTP e persistência de sessão
- ✅ **Leitura de Planilha Excel** com validação de dados
- ✅ **Pesquisa Automática** de DC e Sequencial
- ✅ **Cadastro de Recursos** (Material/Serviço, Fornecimento, Contrato)
- ✅ **Verificação de Códigos** com inserção automática de Item Não Orçado
- ✅ **Geração de Relatório** com status de cada cadastro
- ✅ **Persistência de Sessão** para não precisar logar novamente
- ✅ **Interface Amigável** com logs em tempo real e barra de progresso
- ✅ **Modo Headless** (execução sem interface gráfica do navegador)

## 🛠️ Tecnologias Utilizadas

| Tecnologia | Versão | Finalidade |
|------------|--------|------------|
| .NET | 8.0 | Framework principal |
| C# | 12.0 | Linguagem de programação |
| Windows Forms | 8.0 | Interface gráfica |
| Playwright | 1.40+ | Automação web |
| ClosedXML | 0.102+ | Manipulação de Excel |
| Microsoft.Extensions.Configuration | 8.0 | Configurações |

## 📦 Pré-requisitos

- Windows 10 ou superior
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (para desenvolvimento)
- [Google Chrome](https://www.google.com/chrome/) ou Microsoft Edge
- Acesso à internet (para primeiro acesso e download de navegadores)

## 🔧 Instalação

### Para desenvolvimento

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/CadastroProducaoCRE.git

# Entre na pasta do projeto
cd CadastroProducaoCRE

# Restaure os pacotes
dotnet restore

# Compile o projeto
dotnet build

# Execute a aplicação
dotnet run
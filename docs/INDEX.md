# LibreHardwareMonitor — Guia Master do Projeto e Navegacao de Documentacao

> **Versao:** 0.9.6.0 (Fork hukapa)  
> **Arquitetura:** .NET 8.0 LTS (`net8.0-windows`) + WinForms + Web Server HTTP Embutido  
> **Publico:** Desenvolvedores, Engenheiros de Software e Agentes de IA  
> **Regra Fundamental:** Zero Emojis em toda a documentacao, codigo e commits.  

---

## 1. Visao Geral do Sistema

O **LibreHardwareMonitor (Fork hukapa)** e uma suite avancada, moderna e resiliente de monitorizacao de hardware para Windows, desenhada para inspecionar em tempo real os sensores de Motherboard, Processador (CPU), Placa Grafica (GPU), Memoria RAM, Discos (HDD/SSD/NVMe), Controladores de Ventoinhas, Fontes de Alimentacao (PSU) e Baterias.

Este fork mantem o foco na modernizacao do ecossistema para .NET 8.0 LTS, melhorias de usabilidade na interface desktop, desacoplamento arquitetural e um servidor web embutido completamente redesenhado.

### Capacidades Nucleares
1. **Monitorizacao Abrangente de Sensores:** Leitura de temperaturas, tensoes, frequencias de relogio, potencias (Watts), percentagens de uso, taxas de fluxo e nivel de desgaste.
2. **Minimizar para Segundo Plano ao Clicar no "X":** O encerramento no botao "X" da janela principal coloca a aplicacao em segundo plano na barra de tarefas (System Tray), mantendo a monitorizacao e o servidor web ativos sem interrupcao.
3. **Dashboard Web Remoto Moderno:** Interface web zero-dependencies servida diretamente pelo executavel via HTTP (porta 8085 por omissao), com deteccao nativa de Dark/Light mode (`light-dark()`), cards responsivos e pesquisa instantanea em tempo real.
4. **Resiliencia ao Windows 11 Smart App Control (SAC):** Isolamento de drivers de terceiros nao assinados com carregamento tardio protegido (`NoInlining`), assegurando que a aplicacao inicia mesmo em ambientes com politicas de seguranca empresariais ativas.
5. **Arquitetura Modular em Classes Parciais:** Decomposicao de classes monoliticas (como `MainForm.cs`), respeitando o Principio da Responsabilidade Unica (SRP) e o limite de linhas.

---

## 2. Mapa e Indice de Documentacao (`/docs`)

A documentacao esta estruturada tematicamente no diretorio `/docs`:

```text
LibreHardwareMonitor/
├── docs/
│   ├── INDEX.md                             # Este ficheiro: Guia Master e sitemap do projeto
│   │
│   ├── architecture/
│   │   └── FILE_TREE.md                     # Registo mestre de consulta rapida: mapa de ficheiros, funcoes e limites
│   │
│   ├── reports/
│   │   └── RELATORIO_ESTADO_E_MELHORIAS.md  # Relatorio tecnico detalhado (.NET 8, Smart App Control, Tray, Web)
│   │
│   ├── history/
│   │   └── DOCUMENTATION.md                 # Historico cronologico de todas as alteracoes e sessoes de engenharia
│   │
│   └── agents/
│       └── AGENTS.md                        # Playbook para Agentes de IA: padroes DRY/KISS, regras de codigo e Zero Emojis
```

---

## 3. Padrao Arquitetural e Limites Rigorosos de Linhas

Para manter a codebase manutenivel, legivel e facilmente operavel por humanos e agentes:
* **Classes C# (.cs):** Limite recomendado de **250 a 450 linhas** por ficheiro ou classe parcial.
* **Ficheiros Web (.html, .css, .js):** Limite de **300 a 400 linhas**, mantendo modularidade e zero dependencias pesadas.
* **Principio da Responsabilidade Unica (SRP):** Cada ficheiro cumpre uma finalidade logica bem delimitada.
* **Regra Zero Emojis:** Todos os artefactos, codigos, mensagens de commit e documentacoes utilizam texto formal tecnico sem iconografia emoji.

---

## 4. Estrutura Modular da Solucao

```text
LibreHardwareMonitor/
├── Aga.Controls/                            # Controlo WinForms TreeViewAdv com suporte multicoluna para sensores
├── LibreHardwareMonitorLib/                 # Biblioteca central de deteccao de hardware e leitura de sensores
│   ├── Hardware/                            # Deteccao por categorias (CPU, GPU, Memory, Storage, etc.)
│   ├── Interop/                             # PInvoke e chamadas nativas Win32
│   └── PawnIO/                              # Modulos de comunicacao I/O de baixo nivel
├── LibreHardwareMonitor.Windows.Forms/      # Interface desktop principal WinForms
│   ├── UI/                                  # Formularios, controlos e classes parciais de MainForm
│   │   ├── MainForm.cs                      # Ciclo de vida principal, construtor e estado
│   │   ├── MainForm.Tray.cs                 # Integracao com a bandeja do sistema e intercepcao de fecho
│   │   ├── MainForm.WebServer.cs            # Controlo e configuracao do servidor web
│   │   ├── MainForm.Plot.cs                 # Graficos, escalas e selecao de sensores
│   │   └── MainForm.Tree.cs                 # Arvore de sensores, menus de contexto e eventos de rato
│   ├── Utilities/                           # HttpServer embutido, configuracoes persistentes e logger
│   └── Resources/                           # Recursos graficos, manifest e dashboard Web
└── Iniciar-LibreHardwareMonitor.bat         # Script de arranque com elevacao administrativa e WorkingDirectory
```

---

## 5. Como Compilar e Executar

### Pre-requisitos
* Windows 10/11 x64
* .NET 8.0 SDK (versao 8.0.400 ou superior)

### Compilacao da Versao Release (.NET 8.0)
```powershell
dotnet build LibreHardwareMonitor.Windows.Forms\LibreHardwareMonitor.Windows.Forms.csproj -c Release /p:Platform=x64
```

### Publicacao Standalone em Ficheiro Unico (Single-File)
```powershell
dotnet publish LibreHardwareMonitor.Windows.Forms\LibreHardwareMonitor.Windows.Forms.csproj -c Release -f net8.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=true -p:Platform=x64 -o "bin\Publish-SingleFile"
```

### Execucao
Execute o ficheiro `Iniciar-LibreHardwareMonitor.bat` para iniciar a aplicacao com privilegios de administrador necessarios a leitura de todos os sensores de motherboard e processador.

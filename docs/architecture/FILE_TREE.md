# LibreHardwareMonitor — Arvore de Ficheiros e Registo Arquitetural

> **Finalidade:** Registo central de consulta rapida de toda a codebase do LibreHardwareMonitor.  
> **Regra de Limites de Linhas:** Ficheiros devem respeitar o intervalo de **250 a 450 linhas** sempre que possivel.  
> **Regra Fundamental:** Zero Emojis e responsabilidade unica (SRP).  

---

## 1. Mapa Global da Solucao

```text
LibreHardwareMonitor/
├── docs/                                          # Hub Central de Documentacao
│   ├── INDEX.md                                   # Guia Master de navegacao e sitemap
│   ├── architecture/
│   │   └── FILE_TREE.md                           # Este ficheiro: registo rapido de todos os ficheiros e funcoes
│   ├── reports/
│   │   └── RELATORIO_ESTADO_E_MELHORIAS.md        # Relatorio tecnico detalhado (SAC, .NET 8, Tray, Web)
│   ├── history/
│   │   └── DOCUMENTATION.md                       # Historico cronologico de engenharia
│   └── agents/
│       └── AGENTS.md                              # Regras e padroes para programadores e agentes
│
├── LibreHardwareMonitor.Windows.Forms/            # Projeto Desktop WinForms Principal
│   ├── Program.cs                                 # Ponto de entrada, verificacao de ficheiros e crash handler
│   ├── Resources/
│   │   ├── app.manifest                           # Manifest de compatibilidade OS e nivel de execucao (asInvoker)
│   │   └── Web/
│   │       └── index.html                         # Dashboard web moderno embutido (HTML5/CSS3/ES6 dark mode)
│   ├── UI/
│   │   ├── MainForm.cs                            # Classe parcial: Construtor, ciclo de vida e estado
│   │   ├── MainForm.Designer.cs                   # Controlos graficos gerados pelo designer WinForms
│   │   ├── MainForm.Tray.cs                       # Classe parcial: Bandeja do sistema e minimizar ao fechar 'X'
│   │   ├── MainForm.WebServer.cs                  # Classe parcial: Controlo do servidor web e dialogos de porta/auth
│   │   ├── MainForm.Plot.cs                       # Classe parcial: Painel de graficos, selecao e sincronizacao
│   │   ├── MainForm.Tree.cs                       # Classe parcial: Gestao de TreeViewAdv, colunas e contexto
│   │   ├── SensorGadget.cs                        # Gadget flutuante de sensores no ambiente de trabalho
│   │   ├── SystemTray.cs                          # Gestor de icones na area de notificacao do Windows
│   │   ├── PlotPanel.cs                           # Painel de desenho com biblioteca OxyPlot
│   │   ├── ScaledPlotModel.cs                     # Modelo escalonado para visualizacao temporal de sensores
│   │   ├── Node.cs                                # No base da arvore de exibicao
│   │   ├── HardwareNode.cs                        # No que encapsula um componente de hardware
│   │   ├── SensorNode.cs                          # No que encapsula um sensor individual
│   │   ├── TypeNode.cs                            # No agrupador por tipo de sensor (Temperatura, Tensao, etc.)
│   │   ├── UserOption.cs                          # Mapeamento bidirecional de definicoes booleanas em menus
│   │   ├── UserRadioGroup.cs                      # Mapeamento de grupos de selecao mutualmente exclusivos
│   │   ├── UnitManager.cs                         # Gestor de unidades de medida (Celsius vs Fahrenheit)
│   │   ├── AboutBox.cs                            # Dialogo "Sobre" com informacoes de versao e creditos
│   │   ├── AuthForm.cs                            # Dialogo para definicao de credenciais do servidor web
│   │   ├── InterfacePortForm.cs                   # Dialogo para definicao da porta e IP de escuta HTTP
│   │   └── ParameterForm.cs                       # Dialogo para ajuste fino de parametros de calibracao
│   ├── Utilities/
│   │   ├── HttpServer.cs                          # Servidor HTTP embutido que entrega JSON e o dashboard web
│   │   ├── PersistentSettings.cs                  # Leitura e escrita de definicoes em ficheiro XML/config
│   │   ├── Logger.cs                              # Registo historico de leituras em ficheiro CSV
│   │   ├── StartupManager.cs                      # Configuracao de arranque automatico com o Windows
│   │   └── EmbeddedResources.cs                   # Extracao de imagens e recursos binarios embutidos
│   └── UI/Themes/
│       ├── Theme.cs                               # Classe abstrata base e gestao do tema ativo (Auto/Dark/Light)
│       ├── DarkTheme.cs                           # Paleta de cores para tema escuro
│       ├── LightTheme.cs                          # Paleta de cores para tema claro
│       ├── ThemedToolStripRenderer.cs             # Renderizador visual customizado para menus
│       └── ThemedScrollIndicators.cs              # Barras de deslocamento estilizadas
│
├── LibreHardwareMonitorLib/                       # Biblioteca Nuclear de Leitura de Hardware
│   ├── Hardware/
│   │   ├── Computer.cs                            # Fachada central: gestao de grupos e traversal de visitantes
│   │   ├── IComputer.cs                           # Interface publica de acesso aos grupos de hardware
│   │   ├── IHardware.cs                           # Contrato para qualquer componente de hardware detetado
│   │   ├── ISensor.cs                             # Contrato de um sensor (Value, Min, Max, SensorType)
│   │   ├── Sensor.cs                              # Implementacao base de sensor com historico temporal
│   │   ├── Hardware.cs                            # Implementacao base com suporte a arvore hierarquica
│   │   ├── SensorType.cs                          # Enumeracao (Voltage, Clock, Temperature, Load, Fan, Flow, Power, etc.)
│   │   ├── Cpu/                                   # Leitura de CPU (Intel, AMD Zen/Ryzen) via MSR e PawnIO
│   │   ├── Gpu/                                   # Leitura de placas graficas (Nvidia NVAPI, AMD ADL, Intel)
│   │   ├── Memory/                                # Memoria virtual, memoria fisica total e deteccao SPD
│   │   │   ├── MemoryGroup.cs                     # Inicializacao protegida de memoria RAM
│   │   │   ├── VirtualMemory.cs                   # Estatisticas de RAM disponivel e em uso via Windows API
│   │   │   ├── TotalMemory.cs                     # Capacidade total de RAM instalada
│   │   │   └── DimmMemory.cs                      # Leitura de temperatura e SPD de modulos individuais
│   │   ├── Motherboard/                           # Chips Super I/O (ITE, Nuvoton, Winbond, Fintek, SMSC)
│   │   ├── Storage/                               # SMART de discos HDD, SSD SATA e NVMe
│   │   ├── Controller/                            # Controladores externos (AquaComputer, NZXT, Corsair, Razer, Arctic)
│   │   ├── Network/                               # Placas de rede (taxas de download e upload via Performance Counters)
│   │   ├── Psu/                                   # Fontes de alimentacao inteligentes (Corsair, MSI)
│   │   └── Battery/                               # Baterias de portateis (capacidade restante, taxa de descarga)
│   ├── Interop/
│   │   ├── Kernel32.cs                            # Chamadas Win32 nativas para memoria e processos
│   │   └── SetupApi.cs                            # Enumeracao de dispositivos de hardware do Windows
│   └── PawnIO/                                    # Suporte ao driver moderno PawnIO para acesso Ring-0
│
└── Aga.Controls/                                  # Componente WinForms TreeViewAdv Multicoluna
    ├── Tree/
    │   ├── TreeViewAdv.cs                         # Controlo de arvore com renderizacao virtualizada de alto desempenho
    │   ├── TreeColumn.cs                          # Definicao de coluna (largura, cabecalho, visibilidade)
    │   ├── TreeModel.cs                           # Modelo de dados hierarquico que alimenta a arvore
    │   └── NodeControls/                          # Controlos embutidos nas celulas (CheckBox, TextBox, Icon)
    └── Threading/
        └── AbortableThreadPool.cs                 # Pool de threads para tarefas assincronas de interface
```

---

## 2. Registo Detalhado de Responsabilidades por Ficheiro

### 2.1 Modulo de Interface Desktop (`LibreHardwareMonitor.Windows.Forms`)

| Ficheiro | Responsabilidade Principal | Limite de Linhas | Dependencias Criticas |
|---|---|---|---|
| `Program.cs` | Ponto de entrada, verificacao de integridade de ficheiros e captura global de excecoes para `crash.log`. | <= 100 lin | `System.Windows.Forms`, `MainForm` |
| `UI/MainForm.cs` | Orquestracao principal do formulário, ciclo de vida, construtor, carregamento de definicoes e despacho de atualizacoes. | <= 450 lin | `Computer`, `PersistentSettings`, `TreeModel` |
| `UI/MainForm.Tray.cs` | Controlo da barra de notificacoes (System Tray), intercepcao do comando fechar no botao "X" (`SC_CLOSE`) e saida definitiva. | <= 150 lin | `SystemTray`, `FormClosingEventArgs`, `WndProc` |
| `UI/MainForm.WebServer.cs` | Ativacao, paragem e configuracao da porta de escuta e credenciais de seguranca do servidor HTTP. | <= 100 lin | `HttpServer`, `InterfacePortForm`, `AuthForm` |
| `UI/MainForm.Plot.cs` | Sincronizacao do formulario ou painel embutido de graficos, selecao de cores de sensores e rotacao de logs. | <= 300 lin | `PlotPanel`, `OxyPlot`, `Logger` |
| `UI/MainForm.Tree.cs` | Gestao de eventos da arvore `TreeViewAdv`, redimensionamento de colunas, cliques, atalhos de teclado e menu de contexto de sensores. | <= 400 lin | `TreeViewAdv`, `SensorNode`, `HardwareNode` |
| `Utilities/HttpServer.cs` | Servidor HTTP multithreaded que escuta requisicoes REST (`/data.json`) e serve o dashboard web embutido (`/` ou `/index.html`). | <= 450 lin | `HttpListener`, `IElement`, `Node` |
| `Utilities/PersistentSettings.cs` | Mecanismo de persistencia de configuracoes em XML com fallback automatico para ficheiro `.backup`. | <= 250 lin | `System.Xml`, `ISettings` |

---

### 2.2 Modulo Nuclear de Monitorizacao (`LibreHardwareMonitorLib`)

| Ficheiro | Responsabilidade Principal | Mecanismo de Acesso |
|---|---|---|
| `Hardware/Computer.cs` | Ponto de entrada para deteccao de hardware, ativacao dinamica de grupos e percurso de arvore de sensores. | Fachada / Visitor Pattern |
| `Hardware/Memory/MemoryGroup.cs` | Monitorizacao de memoria RAM do sistema. Possui isolamento `NoInlining` contra bloqueios de politicas de seguranca. | Win32 GlobalMemoryStatusEx + PawnIO |
| `Hardware/Cpu/CpuGroup.cs` | Deteccao de processadores Intel Core e AMD Ryzen/EPYC, tensoes, temperaturas por nucleo e potencias de pacote. | MSRs (Model-Specific Registers) via PawnIO |
| `Hardware/Gpu/GpuGroup.cs` | Deteccao de placas de video dedicadas e integradas, frequencias de motor, VRAM e curvas de ventilacao. | NVAPI (Nvidia), ADL (AMD), Intel IGFX |
| `Hardware/Motherboard/MotherboardGroup.cs` | Leitura de chips Super I/O em placas-mae (voltagens de linhas +12V, +5V, +3.3V, RPMs de conectores FAN). | Portas LPC I/O via PawnIO |
| `Hardware/Storage/StorageGroup.cs` | Extracao de dados SMART de unidades de estado solido e discos rigidos, saude e temperaturas. | Win32 DeviceIoControl / NVMe pass-through |
| `Hardware/Controller/` | Suporte a controladores USB proprietarios de marcas populares de arrefecimento liquido e ventoinhas. | HidSharp / USB HID Reports |

---

### 2.3 Modulo de Dashboard Web (`Resources/Web/index.html`)

| Caracteristica | Especificacao Tecnica |
|---|---|
| Dependencias | 0 bibliotecas externas (Vanilla HTML5, CSS3 Moderno, JavaScript ES2022). |
| Temas | Deteccao automatica de Dark Mode / Light Mode do sistema via CSS `color-scheme: light dark` e `light-dark()`. |
| Responsividade | Layout baseado em CSS Grid adaptativo (Mobile, Tablet e Monitores Ultra-Wide). |
| Atualizacao | Polling assincrono via `fetch('/data.json')` a cada 1.5 segundos com tratamento de erros. |
| Pesquisa | Filtro em tempo real de hardware e sensores diretamente na interface. |

---

## 3. Padrões de Interop e Tratamento de Exceções

1. **Protecao de Carregamento Dinamico (Smart App Control):**  
   Nenhum tipo de driver de terceiros nao assinado pode ser instanciado diretamente no construtor de uma classe publica. Deve ser sempre isolado num metodo privado anotado com:
   ```csharp
   [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
   ```
   e envolvido por blocos `try / catch (Exception)`.

2. **Fecho em Segundo Plano (Minimize to Tray):**  
   O encerramento do formulario no 'X' deve sempre verificar `e.CloseReason == CloseReason.UserClosing && _minimizeOnClose.Value`. Se ativo, cancela o evento (`e.Cancel = true`) e altera `Visible = false`. O fecho definitivo apenas ocorre atraves da acao explicita `CloseApplication()`.

# Historico de Engenharia — Cronologia de Evolucao Tecnica

> **Projeto:** LibreHardwareMonitor (Fork hukapa)  
> **Regra Fundamental:** Zero Emojis  

---

## 1. Fase 1: Criacao do Fork e Configuracao do Ambiente

* **Objetivo:** Estabelecer o repositorio fork em `hukapa/LibreHardwareMonitor` e configurar os remotos Git locais.
* **Acoes Efetuadas:**
  - Clonagem para o diretorio de desenvolvimento: `C:\Users\tiago\Documents\GitHub\CrystalStudio-AI\LibreHardwareMonitor`.
  - Configuracao dos remotos:
    - `origin`: `https://github.com/hukapa/LibreHardwareMonitor.git`
    - `upstream`: `https://github.com/LibreHardwareMonitor/LibreHardwareMonitor.git`
  - Definicao do fluxo de trabalho no GitHub Desktop para "For my own purposes", garantindo commits e publicacao direta no fork.

---

## 2. Fase 2: Modernizacao para .NET 8.0 LTS

* **Problema Identificado:**  
  O repositorio upstream havia saltado para versoes experimentais do .NET 10 (`net10.0`), que nao estao disponiveis no ambiente de producao do utilizador (equipado com .NET 8.0.427). Por outro lado, manter o projeto em .NET Framework 4.7.2 representava manter divida tecnica com bibliotecas e runtime obsoletos de 2018.
* **Acoes Efetuadas:**
  - Atualizacao de `LibreHardwareMonitorLib.csproj`, `Aga.Controls.csproj` e `LibreHardwareMonitor.Windows.Forms.csproj` para suportar `net8.0-windows` e `net8.0`.
  - Correcao do gerador de codigo nativo CsWin32 adicionando `<PlatformTarget>x64</PlatformTarget>` e `<CsWin32PlatformTarget>x64</CsWin32PlatformTarget>`.
  - Retrocesso de sintaxe C# 13 preview (`field` keyword em `Sensor.cs` e atribuicao null-condicional de l-value em `ArcticFanController.cs`) para padroes estaveis do C# 12 / .NET 8.
  - Alinhamento de versoes do pacote NuGet `System.Resources.Extensions` para eliminar avisos `NU1605`.

---

## 3. Fase 3: Funcionalidade Minimizar para a Bandeja no 'X'

* **Objetivo:** Fazer com que o encerramento da janela principal coloque a aplicacao em segundo plano no System Tray em vez de terminar o processo.
* **Acoes Efetuadas:**
  - Criacao da opcao persistente `_minimizeOnClose` com valor padrao `true`.
  - Sobrescrita de `OnFormClosing` para detetar `CloseReason.UserClosing`, aplicando `e.Cancel = true`, ocultando o formulario com `Visible = false` e mantendo o icone da bandeja ativo.
  - Intercepcao da mensagem de sistema `SC_CLOSE` em `WndProc` para direcionamento imediato para `SysTrayHideShow()`.
  - Restauracao automatica do estado da janela para `FormWindowState.Normal` ao clicar no icone da bandeja do sistema quando a aplicacao se encontra minimizada.

---

## 4. Fase 4: Modernizacao Completa do Servidor Web

* **Objetivo:** Substituir o dashboard web antigo de 2012 por uma solucao moderna e responsiva.
* **Acoes Efetuadas:**
  - Substituicao de jQuery 1.7 e Knockout.js por JavaScript ES2022 nativo sem dependencias.
  - Criacao de folha de estilos contemporanea com suporte automatico a tema claro e escuro (`light-dark()`, `color-scheme: light dark`).
  - Layout em cartoes responsivos por dispositivo de hardware com atualizacao automatica a cada 1.5s via endpoint REST `/data.json`.
  - Adicao de barra de pesquisa instantanea para filtragem de sensores em tempo real.

---

## 5. Fase 5: Diagnostico e Blindagem do Smart App Control (Windows 11)

* **Problema Identificado:**  
  O Controlo de Aplicacoes Inteligente do Windows 11 bloqueava `RAMSPDToolkit-NDD.dll` com erro `0x800711C7`, provocando encerramento silencioso no arranque. Adicionalmente, o manifest com `requireAdministrator` gerava erro 740 ("O pedido nao e suportado") ao ser chamado sem elevacao explicita.
* **Acoes Efetuadas:**
  - Isolamento com `[MethodImpl(MethodImplOptions.NoInlining)]` e `try/catch` no `MemoryGroup.cs`.
  - Protecao global em `Computer.cs` para adicao resiliente de grupos de hardware.
  - Adicao de captura global de falhas em `Program.cs` com registo em `crash.log`.
  - Alteracao do manifest para `asInvoker` e atualizacao do script `Iniciar-LibreHardwareMonitor.bat` com `-WorkingDirectory`.
  - Suporte a empacotamento standalone em ficheiro unico (`PublishSingleFile=true`).

---

## 6. Fase 6: Decomposicao Modular de `MainForm.cs` e Hub `docs/`

* **Objetivo:** Eliminar a classe monolitica de 1.479 linhas, modularizar por SRP e criar o registo de documentacao do projeto no padrao CrystalStudio AI.
* **Acoes Efetuadas:**
  - Criacao da pasta `docs/` com os modulos `INDEX.md`, `FILE_TREE.md`, `RELATORIO_ESTADO_E_MELHORIAS.md`, `DOCUMENTATION.md` e `AGENTS.md`.
  - Decomposicao de `MainForm.cs` em 5 classes parciais (`MainForm.cs`, `MainForm.Tray.cs`, `MainForm.WebServer.cs`, `MainForm.Plot.cs`, `MainForm.Tree.cs`), respeitando limites rigorosos de linhas e o principio da responsabilidade unica.
  - Validacao de compilacao: 0 erros e 0 avisos bloqueantes.

---

## 7. Fase 7: Resolucao de Assemblies Embutidos no Modo Single-File

* **Problema Identificado:**  
  Ao iniciar a versao compilada em ficheiro unico (`bin\Publish-SingleFile\LibreHardwareMonitor.Windows.Forms.exe`), surgia a caixa de dialogo `"Error: The following file could not be found: Aga.Controls.dll. Please extract all files from the archive."`. A verificacao legada em `Program.cs` utilizava `File.Exists(path + fileName)` buscando ficheiros `.dll` soltos em disco, ignorando que no modo Single-File todos os assemblies geridos estao embutidos em memoria no proprio binario executavel.
* **Acoes Efetuadas:**
  - Substituicao de `IsFileAvailable(string fileName)` por `IsAssemblyAvailable(string assemblyName)`.
  - Resolucao de assemblies atraves de `AppDomain.CurrentDomain.GetAssemblies()` e `Assembly.Load(new AssemblyName(assemblyName))` com fallback seguro para verificacao de ficheiro fisico em disco.
  - Re-publicacao do executavel standalone com sucesso e validacao de arranque sem erros em ambiente de producao.

#requires -Version 5.1

[CmdletBinding()]
param(
    [string] $Root = '',
    [switch] $ValidateOnly,
    [string] $PreviewPath = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..\..')).Path
}

Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

$xaml = @'
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="SHITSTORM · Git Watch"
    Width="1320"
    Height="790"
    MinWidth="1040"
    MinHeight="620"
    WindowStartupLocation="CenterScreen"
    Background="#0B0E14"
    Foreground="#EDF1F7"
    FontFamily="Segoe UI">

    <Window.Resources>
        <SolidColorBrush x:Key="PanelBrush" Color="#121722" />
        <SolidColorBrush x:Key="PanelRaisedBrush" Color="#171D29" />
        <SolidColorBrush x:Key="BorderBrush" Color="#283042" />
        <SolidColorBrush x:Key="MutedBrush" Color="#8E99AA" />
        <SolidColorBrush x:Key="AccentBrush" Color="#6B8CFF" />

        <Style x:Key="ActionButton" TargetType="Button">
            <Setter Property="Foreground" Value="#E8EDF6" />
            <Setter Property="Background" Value="#1B2230" />
            <Setter Property="BorderBrush" Value="#303A4D" />
            <Setter Property="BorderThickness" Value="1" />
            <Setter Property="FontSize" Value="13" />
            <Setter Property="FontWeight" Value="SemiBold" />
            <Setter Property="Padding" Value="16,10" />
            <Setter Property="Cursor" Value="Hand" />
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border x:Name="ButtonBorder"
                                Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                CornerRadius="8">
                            <ContentPresenter HorizontalAlignment="Center"
                                              VerticalAlignment="Center"
                                              Margin="{TemplateBinding Padding}" />
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter TargetName="ButtonBorder" Property="Background" Value="#252E40" />
                                <Setter TargetName="ButtonBorder" Property="BorderBrush" Value="#4B5A76" />
                            </Trigger>
                            <Trigger Property="IsPressed" Value="True">
                                <Setter TargetName="ButtonBorder" Property="Opacity" Value="0.78" />
                            </Trigger>
                            <Trigger Property="IsEnabled" Value="False">
                                <Setter TargetName="ButtonBorder" Property="Opacity" Value="0.42" />
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>

        <Style x:Key="PrimaryButton" TargetType="Button" BasedOn="{StaticResource ActionButton}">
            <Setter Property="Background" Value="#365EDC" />
            <Setter Property="BorderBrush" Value="#5679EB" />
        </Style>

        <Style x:Key="SafeButton" TargetType="Button" BasedOn="{StaticResource ActionButton}">
            <Setter Property="Background" Value="#174C3C" />
            <Setter Property="BorderBrush" Value="#29735B" />
        </Style>

        <Style x:Key="SummaryCard" TargetType="Border">
            <Setter Property="Background" Value="{StaticResource PanelBrush}" />
            <Setter Property="BorderBrush" Value="{StaticResource BorderBrush}" />
            <Setter Property="BorderThickness" Value="1" />
            <Setter Property="CornerRadius" Value="10" />
            <Setter Property="Padding" Value="16,12" />
            <Setter Property="Margin" Value="0,0,10,0" />
        </Style>

        <Style TargetType="DataGridColumnHeader">
            <Setter Property="Background" Value="#10151E" />
            <Setter Property="Foreground" Value="#7F8A9C" />
            <Setter Property="BorderBrush" Value="#222A38" />
            <Setter Property="BorderThickness" Value="0,0,0,1" />
            <Setter Property="FontSize" Value="11" />
            <Setter Property="FontWeight" Value="SemiBold" />
            <Setter Property="Padding" Value="12,12" />
        </Style>

        <Style TargetType="DataGridRow">
            <Setter Property="Background" Value="#121722" />
            <Setter Property="Foreground" Value="#E8EDF6" />
            <Setter Property="BorderBrush" Value="#202736" />
            <Setter Property="BorderThickness" Value="0,0,0,1" />
            <Setter Property="MinHeight" Value="54" />
            <Setter Property="ToolTip" Value="{Binding Tooltip}" />
            <Style.Triggers>
                <Trigger Property="AlternationIndex" Value="1">
                    <Setter Property="Background" Value="#141A25" />
                </Trigger>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#1B2332" />
                </Trigger>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="#202D48" />
                </Trigger>
            </Style.Triggers>
        </Style>

        <Style TargetType="DataGridCell">
            <Setter Property="BorderThickness" Value="0" />
            <Setter Property="Padding" Value="12,9" />
            <Setter Property="VerticalContentAlignment" Value="Center" />
            <Setter Property="FocusVisualStyle" Value="{x:Null}" />
        </Style>

        <Style TargetType="TextBox">
            <Setter Property="Background" Value="#10151E" />
            <Setter Property="Foreground" Value="#E8EDF6" />
            <Setter Property="CaretBrush" Value="#E8EDF6" />
            <Setter Property="BorderBrush" Value="#2B3547" />
            <Setter Property="BorderThickness" Value="1" />
            <Setter Property="Padding" Value="12,8" />
            <Setter Property="FontSize" Value="13" />
        </Style>
    </Window.Resources>

    <Grid Margin="24,20,24,16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="18" />
            <RowDefinition Height="82" />
            <RowDefinition Height="18" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="12" />
            <RowDefinition Height="*" />
            <RowDefinition Height="42" />
        </Grid.RowDefinitions>

        <Grid Grid.Row="0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>

            <StackPanel VerticalAlignment="Center">
                <StackPanel Orientation="Horizontal">
                    <Border Width="10" Height="10" CornerRadius="5" Background="#6B8CFF" Margin="0,1,10,0" />
                    <TextBlock Text="SHITSTORM" FontSize="22" FontWeight="Bold" />
                    <TextBlock Text="  /  GIT WATCH" FontSize="15" FontWeight="SemiBold" Foreground="#7F8A9C" VerticalAlignment="Center" />
                </StackPanel>
                <TextBlock x:Name="RootLabel" Margin="20,5,0,0" Foreground="#6F7989" FontSize="12" TextTrimming="CharacterEllipsis" />
            </StackPanel>

            <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
                <CheckBox x:Name="AutoFetchCheckBox"
                          Content="Auto · 5 min"
                          IsChecked="True"
                          Foreground="#AAB3C2"
                          VerticalAlignment="Center"
                          Margin="0,0,18,0"
                          ToolTip="Actualise automatiquement les informations distantes toutes les 5 minutes." />
                <Button x:Name="StatusButton" Style="{StaticResource ActionButton}" Content="Vérifier" Margin="0,0,8,0"
                        ToolTip="Relit immédiatement les états locaux, sans accès réseau." />
                <Button x:Name="FetchButton" Style="{StaticResource PrimaryButton}" Content="↻  Fetch global" Margin="0,0,8,0"
                        ToolTip="Télécharge les informations distantes de tous les dépôts, sans modifier les fichiers." />
                <Button x:Name="PullButton" Style="{StaticResource SafeButton}" Content="↓  Mettre à jour"
                        ToolTip="Met uniquement à jour les dépôts propres et en retard. Les dépôts modifiés ou divergents sont ignorés." />
            </StackPanel>
        </Grid>

        <Grid Grid.Row="2">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>

            <Border Grid.Column="0" Style="{StaticResource SummaryCard}">
                <StackPanel>
                    <TextBlock Text="DÉPÔTS" Foreground="#7F8A9C" FontSize="10" FontWeight="SemiBold" />
                    <TextBlock x:Name="TotalCount" Text="—" FontSize="25" FontWeight="Bold" Margin="0,3,0,0" />
                </StackPanel>
            </Border>
            <Border Grid.Column="1" Style="{StaticResource SummaryCard}">
                <StackPanel>
                    <TextBlock Text="À JOUR" Foreground="#7F8A9C" FontSize="10" FontWeight="SemiBold" />
                    <TextBlock x:Name="CleanCount" Text="—" Foreground="#61D6A3" FontSize="25" FontWeight="Bold" Margin="0,3,0,0" />
                </StackPanel>
            </Border>
            <Border Grid.Column="2" Style="{StaticResource SummaryCard}">
                <StackPanel>
                    <TextBlock Text="MODIFIÉS" Foreground="#7F8A9C" FontSize="10" FontWeight="SemiBold" />
                    <TextBlock x:Name="ModifiedCount" Text="—" Foreground="#EAC66B" FontSize="25" FontWeight="Bold" Margin="0,3,0,0" />
                </StackPanel>
            </Border>
            <Border Grid.Column="3" Style="{StaticResource SummaryCard}">
                <StackPanel>
                    <TextBlock Text="EN RETARD" Foreground="#7F8A9C" FontSize="10" FontWeight="SemiBold" />
                    <TextBlock x:Name="BehindCount" Text="—" Foreground="#FFB45E" FontSize="25" FontWeight="Bold" Margin="0,3,0,0" />
                </StackPanel>
            </Border>
            <Border Grid.Column="4" Style="{StaticResource SummaryCard}" Margin="0">
                <StackPanel>
                    <TextBlock Text="À VÉRIFIER" Foreground="#7F8A9C" FontSize="10" FontWeight="SemiBold" />
                    <TextBlock x:Name="AttentionCount" Text="—" Foreground="#FF6B7A" FontSize="25" FontWeight="Bold" Margin="0,3,0,0" />
                </StackPanel>
            </Border>
        </Grid>

        <Grid Grid.Row="4">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="260" />
            </Grid.ColumnDefinitions>
            <TextBlock Text="ÉTAT DES DÉPÔTS" Foreground="#9AA5B5" FontSize="11" FontWeight="SemiBold" VerticalAlignment="Center" />
            <TextBox x:Name="SearchBox" Grid.Column="1" Text="" ToolTip="Filtrer par dépôt, branche ou auteur" />
            <TextBlock x:Name="SearchHint" Grid.Column="1" Text="Rechercher…" Foreground="#667185" Margin="13,0,0,0"
                       VerticalAlignment="Center" IsHitTestVisible="False" />
        </Grid>

        <Border Grid.Row="6" Background="{StaticResource PanelBrush}" BorderBrush="{StaticResource BorderBrush}" BorderThickness="1" CornerRadius="10" ClipToBounds="True">
            <DataGrid x:Name="RepositoryGrid"
                      AutoGenerateColumns="False"
                      IsReadOnly="True"
                      CanUserAddRows="False"
                      CanUserDeleteRows="False"
                      CanUserResizeRows="False"
                      HeadersVisibility="Column"
                      GridLinesVisibility="None"
                      AlternationCount="2"
                      SelectionMode="Single"
                      SelectionUnit="FullRow"
                      Background="#121722"
                      BorderThickness="0"
                      RowHeaderWidth="0"
                      HorizontalScrollBarVisibility="Auto"
                      VerticalScrollBarVisibility="Auto">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="DÉPÔT" Binding="{Binding Name}" Width="155" />
                    <DataGridTextColumn Header="BRANCHE" Binding="{Binding Branch}" Width="145" />
                    <DataGridTextColumn Header="LOCAL" Binding="{Binding LocalText}" Width="118" />
                    <DataGridTemplateColumn Header="SYNCHRO" Width="175">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <Border Background="{Binding StatusBackground}" CornerRadius="10" Padding="9,4" HorizontalAlignment="Left" VerticalAlignment="Center">
                                    <TextBlock Text="{Binding SyncText}" Foreground="{Binding StatusColor}" FontSize="12" FontWeight="SemiBold" />
                                </Border>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                    <DataGridTemplateColumn Header="DERNIER COMMIT" Width="*" MinWidth="250">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <StackPanel>
                                    <TextBlock Text="{Binding Subject}" Foreground="#DEE4EE" TextTrimming="CharacterEllipsis" />
                                    <TextBlock Text="{Binding LastCommit}" Foreground="#737F91" FontSize="11" Margin="0,2,0,0" />
                                </StackPanel>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                    <DataGridTextColumn Header="AUTEUR" Binding="{Binding Author}" Width="125" />
                    <DataGridTextColumn Header="ACTION" Binding="{Binding Operation}" Width="205" />
                </DataGrid.Columns>
            </DataGrid>
        </Border>

        <Grid Grid.Row="7">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <ProgressBar x:Name="ActivityIndicator" Width="92" Height="3" IsIndeterminate="True" Visibility="Collapsed"
                         Foreground="#6B8CFF" Background="#252D3B" VerticalAlignment="Center" Margin="0,0,14,0" />
            <TextBlock x:Name="StatusLabel" Grid.Column="1" Text="Préparation…" Foreground="#818C9E" FontSize="12" VerticalAlignment="Center" />
            <TextBlock Grid.Column="2" Text="Double-clique un dépôt pour l’ouvrir" Foreground="#596476" FontSize="11" VerticalAlignment="Center" />
        </Grid>
    </Grid>
</Window>
'@

$xmlReader = New-Object System.Xml.XmlNodeReader ([xml] $xaml)
$window = [Windows.Markup.XamlReader]::Load($xmlReader)

$rootLabel = $window.FindName('RootLabel')
$statusButton = $window.FindName('StatusButton')
$fetchButton = $window.FindName('FetchButton')
$pullButton = $window.FindName('PullButton')
$autoFetchCheckBox = $window.FindName('AutoFetchCheckBox')
$repositoryGrid = $window.FindName('RepositoryGrid')
$searchBox = $window.FindName('SearchBox')
$searchHint = $window.FindName('SearchHint')
$totalCount = $window.FindName('TotalCount')
$cleanCount = $window.FindName('CleanCount')
$modifiedCount = $window.FindName('ModifiedCount')
$behindCount = $window.FindName('BehindCount')
$attentionCount = $window.FindName('AttentionCount')
$activityIndicator = $window.FindName('ActivityIndicator')
$statusLabel = $window.FindName('StatusLabel')

$resolvedRoot = (Resolve-Path -LiteralPath $Root).Path
$workerScript = Join-Path $PSScriptRoot 'GitWatch.Worker.ps1'
$stateFile = Join-Path ([System.IO.Path]::GetTempPath()) ("shitstorm-git-watch-{0}.json" -f [Guid]::NewGuid().ToString('N'))
$items = New-Object 'System.Collections.ObjectModel.ObservableCollection[object]'
$repositoryGrid.ItemsSource = $items
$rootLabel.Text = $resolvedRoot
$rootLabel.ToolTip = $resolvedRoot

$script:workerProcess = $null
$script:activeMode = ''
$script:lastResult = $null
$script:searchTerm = ''

function Set-BusyState {
    param([bool] $Busy)

    $statusButton.IsEnabled = -not $Busy
    $fetchButton.IsEnabled = -not $Busy
    $pullButton.IsEnabled = -not $Busy
    $activityIndicator.Visibility = if ($Busy) { 'Visible' } else { 'Collapsed' }
}

function Quote-ProcessArgument {
    param([string] $Value)
    return '"' + $Value.Replace('"', '\"') + '"'
}

function Update-RepositoryView {
    param($Result)

    $items.Clear()
    foreach ($repository in @($Result.Repositories | Sort-Object Name)) {
        if ([string]::IsNullOrWhiteSpace($repository.Operation)) {
            $repository.Operation = '—'
        }

        $tooltipParts = New-Object System.Collections.Generic.List[string]
        $tooltipParts.Add($repository.Path)
        if (-not [string]::IsNullOrWhiteSpace($repository.Remote)) { $tooltipParts.Add($repository.Remote) }
        if (-not [string]::IsNullOrWhiteSpace($repository.Error)) { $tooltipParts.Add($repository.Error) }
        $repository | Add-Member -NotePropertyName Tooltip -NotePropertyValue ($tooltipParts -join "`n") -Force
        $items.Add($repository)
    }

    $totalCount.Text = [string] $Result.Summary.Total
    $cleanCount.Text = [string] $Result.Summary.Clean
    $modifiedCount.Text = [string] $Result.Summary.Modified
    $behindCount.Text = [string] $Result.Summary.Behind
    $attentionCount.Text = [string] $Result.Summary.Attention

    $view = [System.Windows.Data.CollectionViewSource]::GetDefaultView($repositoryGrid.ItemsSource)
    $view.Refresh()
}

function Complete-Refresh {
    try {
        if (-not (Test-Path -LiteralPath $stateFile)) {
            throw 'Le moteur de contrôle ne renvoie aucun résultat.'
        }

        $result = Get-Content -LiteralPath $stateFile -Raw -Encoding UTF8 | ConvertFrom-Json
        if (-not $result.Success) {
            throw $result.Error
        }

        $script:lastResult = $result
        Update-RepositoryView -Result $result

        $duration = [Math]::Round(([double] $result.DurationMs / 1000), 1)
        $time = ([DateTime]::Parse($result.GeneratedAt)).ToLocalTime().ToString('HH:mm:ss')
        $failures = @($result.Repositories | Where-Object { $_.Operation -like '*impossible*' }).Count

        if ($result.Mode -eq 'PullSafe') {
            $statusLabel.Text = "Mise à jour terminée · $($result.Summary.Updated) dépôt(s) mis à jour · $($result.Summary.Skipped) protégé(s) · $duration s · $time"
        }
        elseif ($result.Mode -eq 'Fetch') {
            $suffix = if ($failures -gt 0) { " · $failures échec(s)" } else { '' }
            $statusLabel.Text = "Remotes actualisés · $($result.Summary.Total) dépôts · $duration s · $time$suffix"
        }
        else {
            $statusLabel.Text = "État local actualisé · $($result.Summary.Total) dépôts · $duration s · $time"
        }
    }
    catch {
        $statusLabel.Text = "Erreur · $($_.Exception.Message)"
    }
    finally {
        Set-BusyState -Busy $false
        if (Test-Path -LiteralPath $stateFile) {
            Remove-Item -LiteralPath $stateFile -Force -ErrorAction SilentlyContinue
        }
        if ($null -ne $script:workerProcess) {
            $script:workerProcess.Dispose()
            $script:workerProcess = $null
        }
        $script:activeMode = ''
    }
}

function Start-Refresh {
    param([ValidateSet('Status', 'Fetch', 'PullSafe')] [string] $Mode)

    if ($null -ne $script:workerProcess -and -not $script:workerProcess.HasExited) {
        return
    }

    Remove-Item -LiteralPath $stateFile -Force -ErrorAction SilentlyContinue
    $script:activeMode = $Mode
    Set-BusyState -Busy $true

    if ($Mode -eq 'PullSafe') {
        $statusLabel.Text = 'Mise à jour prudente en cours… les dépôts sensibles seront ignorés.'
    }
    elseif ($Mode -eq 'Fetch') {
        $statusLabel.Text = 'Actualisation des remotes en cours…'
    }
    else {
        $statusLabel.Text = 'Lecture des états locaux…'
    }

    $arguments = @(
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-File', $workerScript,
        '-Root', $resolvedRoot,
        '-Mode', $Mode,
        '-OutputPath', $stateFile
    )

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = 'powershell.exe'
    $startInfo.Arguments = (($arguments | ForEach-Object { Quote-ProcessArgument $_ }) -join ' ')
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden

    try {
        $script:workerProcess = [System.Diagnostics.Process]::Start($startInfo)
    }
    catch {
        Set-BusyState -Busy $false
        $statusLabel.Text = "Impossible de démarrer le contrôle : $($_.Exception.Message)"
    }
}

$pollTimer = New-Object System.Windows.Threading.DispatcherTimer
$pollTimer.Interval = [TimeSpan]::FromMilliseconds(300)
$pollTimer.Add_Tick({
    if ($null -ne $script:workerProcess -and $script:workerProcess.HasExited) {
        Complete-Refresh
    }
})

$localRefreshTimer = New-Object System.Windows.Threading.DispatcherTimer
$localRefreshTimer.Interval = [TimeSpan]::FromSeconds(30)
$localRefreshTimer.Add_Tick({
    if ($null -eq $script:workerProcess) {
        Start-Refresh -Mode 'Status'
    }
})

$remoteRefreshTimer = New-Object System.Windows.Threading.DispatcherTimer
$remoteRefreshTimer.Interval = [TimeSpan]::FromMinutes(5)
$remoteRefreshTimer.Add_Tick({
    if ($autoFetchCheckBox.IsChecked -and $null -eq $script:workerProcess) {
        Start-Refresh -Mode 'Fetch'
    }
})

$view = [System.Windows.Data.CollectionViewSource]::GetDefaultView($repositoryGrid.ItemsSource)
$view.Filter = [System.Predicate[object]] {
    param($item)
    if ([string]::IsNullOrWhiteSpace($script:searchTerm)) { return $true }
    $haystack = "$($item.Name) $($item.Branch) $($item.Author) $($item.StatusKey)"
    return $haystack.IndexOf($script:searchTerm, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

$searchBox.Add_TextChanged({
    $script:searchTerm = $searchBox.Text.Trim()
    $searchHint.Visibility = if ($searchBox.Text.Length -eq 0) { 'Visible' } else { 'Collapsed' }
    $view.Refresh()
})

$statusButton.Add_Click({ Start-Refresh -Mode 'Status' })
$fetchButton.Add_Click({ Start-Refresh -Mode 'Fetch' })
$pullButton.Add_Click({ Start-Refresh -Mode 'PullSafe' })

$repositoryGrid.Add_MouseDoubleClick({
    if ($null -ne $repositoryGrid.SelectedItem) {
        Start-Process -FilePath 'explorer.exe' -ArgumentList $repositoryGrid.SelectedItem.Path
    }
})

$window.Add_Loaded({
    if ([string]::IsNullOrWhiteSpace($PreviewPath)) {
        $pollTimer.Start()
        $localRefreshTimer.Start()
        $remoteRefreshTimer.Start()
        Start-Refresh -Mode 'Fetch'
    }
})

$window.Add_Closed({
    $pollTimer.Stop()
    $localRefreshTimer.Stop()
    $remoteRefreshTimer.Stop()
    if ($null -ne $script:workerProcess -and -not $script:workerProcess.HasExited) {
        try { $script:workerProcess.Kill() } catch { }
    }
    Remove-Item -LiteralPath $stateFile -Force -ErrorAction SilentlyContinue
})

if ($ValidateOnly) {
    Write-Output 'UI_OK'
    exit 0
}

if (-not [string]::IsNullOrWhiteSpace($PreviewPath)) {
    $previewStateFile = Join-Path ([System.IO.Path]::GetTempPath()) ("shitstorm-git-watch-preview-{0}.json" -f [Guid]::NewGuid().ToString('N'))
    try {
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $workerScript -Root $resolvedRoot -Mode Status -OutputPath $previewStateFile
        $previewResult = Get-Content -LiteralPath $previewStateFile -Raw -Encoding UTF8 | ConvertFrom-Json
        if (-not $previewResult.Success) { throw $previewResult.Error }

        Update-RepositoryView -Result $previewResult
        $statusLabel.Text = "État local actualisé · $($previewResult.Summary.Total) dépôts · aperçu"
        $window.WindowStartupLocation = 'Manual'
        $window.Left = -20000
        $window.Top = -20000
        $window.ShowInTaskbar = $false
        $window.Show()
        $window.UpdateLayout()
        $window.Dispatcher.Invoke([Action] { }, [System.Windows.Threading.DispatcherPriority]::Render)

        $pixelWidth = [Math]::Max(1, [int] [Math]::Ceiling($window.ActualWidth))
        $pixelHeight = [Math]::Max(1, [int] [Math]::Ceiling($window.ActualHeight))
        $bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap($pixelWidth, $pixelHeight, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
        $bitmap.Render($window)

        $previewDirectory = Split-Path -Parent $PreviewPath
        if (-not (Test-Path -LiteralPath $previewDirectory)) {
            [void] (New-Item -ItemType Directory -Path $previewDirectory -Force)
        }
        $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
        $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
        $stream = [System.IO.File]::Open($PreviewPath, [System.IO.FileMode]::Create)
        try { $encoder.Save($stream) } finally { $stream.Dispose() }
        $window.Close()
        Write-Output $PreviewPath
        exit 0
    }
    finally {
        Remove-Item -LiteralPath $previewStateFile -Force -ErrorAction SilentlyContinue
    }
}

[void] $window.ShowDialog()

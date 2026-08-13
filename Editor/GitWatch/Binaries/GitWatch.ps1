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

        <Style x:Key="PublishButton" TargetType="Button" BasedOn="{StaticResource ActionButton}">
            <Setter Property="Background" Value="#4937A8" />
            <Setter Property="BorderBrush" Value="#705CE0" />
            <Setter Property="Foreground" Value="#F0EDFF" />
        </Style>

        <Style x:Key="WarningButton" TargetType="Button" BasedOn="{StaticResource ActionButton}">
            <Setter Property="Background" Value="#5A3C13" />
            <Setter Property="BorderBrush" Value="#9C6923" />
            <Setter Property="Foreground" Value="#FFD78A" />
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
                <Button x:Name="DiscardButton" Style="{StaticResource WarningButton}" Content="↶  Discard TMP" Margin="0,0,8,0" Visibility="Collapsed"
                        ToolTip="Restaure au dernier commit les atlas TMP identifiés comme vidés automatiquement. Une confirmation détaillée sera demandée." />
                <Button x:Name="StatusButton" Style="{StaticResource ActionButton}" Content="Vérifier" Margin="0,0,8,0"
                        ToolTip="Relit immédiatement les états locaux, sans accès réseau." />
                <Button x:Name="FetchButton" Style="{StaticResource PrimaryButton}" Content="↻  Fetch global" Margin="0,0,8,0"
                        ToolTip="Télécharge les informations distantes de tous les dépôts, sans modifier les fichiers." />
                <Button x:Name="PullButton" Style="{StaticResource SafeButton}" Content="↓  Mettre à jour" Margin="0,0,8,0"
                        ToolTip="Met uniquement à jour les dépôts propres et en retard. Les dépôts modifiés ou divergents sont ignorés." />
                <Button x:Name="CommitPushButton" Style="{StaticResource PublishButton}" Content="↑  Commit &amp; Push" IsEnabled="False"
                        ToolTip="Crée un commit dans chaque dépôt modifié avec le même message, puis tente de le pousser." />
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
                    <DataGridTemplateColumn Header="CHANGEMENTS LOCAUX" Width="160">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <Border Background="{Binding LocalBackground}" CornerRadius="10" Padding="9,4" HorizontalAlignment="Left" VerticalAlignment="Center">
                                    <TextBlock Text="{Binding LocalText}" Foreground="{Binding LocalColor}" FontSize="12" FontWeight="SemiBold" />
                                </Border>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
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
                    <DataGridTemplateColumn Header="ACTION / DIAGNOSTIC" Width="205">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <TextBlock Text="{Binding OperationDisplay}" Foreground="{Binding OperationColor}" TextWrapping="Wrap" />
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                </DataGrid.Columns>
            </DataGrid>
        </Border>

        <Grid Grid.Row="7">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <ProgressBar x:Name="ActivityIndicator" Width="180" Height="4" Minimum="0" Maximum="100" Value="0" IsIndeterminate="True" Visibility="Collapsed"
                         Foreground="#6B8CFF" Background="#252D3B" VerticalAlignment="Center" Margin="0,0,14,0" />
            <TextBlock x:Name="StatusLabel" Grid.Column="1" Text="Préparation…" Foreground="#818C9E" FontSize="12" VerticalAlignment="Center" />
            <TextBlock Grid.Column="2" Text="Survole l’état pour le journal · Double-clique un dépôt pour l’ouvrir" Foreground="#596476" FontSize="11" VerticalAlignment="Center" />
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
$commitPushButton = $window.FindName('CommitPushButton')
$discardButton = $window.FindName('DiscardButton')
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
$progressFile = Join-Path ([System.IO.Path]::GetTempPath()) ("shitstorm-git-watch-progress-{0}.log" -f [Guid]::NewGuid().ToString('N'))
$items = New-Object 'System.Collections.ObjectModel.ObservableCollection[object]'
$repositoryGrid.ItemsSource = $items
$rootLabel.Text = $resolvedRoot
$rootLabel.ToolTip = $resolvedRoot

$script:workerProcess = $null
$script:activeMode = ''
$script:lastResult = $null
$script:searchTerm = ''
$script:progressLines = New-Object System.Collections.Generic.List[string]
$script:progressFileLineCount = 0
$script:progressTotal = 0

function Set-BusyState {
    param([bool] $Busy)

    $statusButton.IsEnabled = -not $Busy
    $fetchButton.IsEnabled = -not $Busy
    $pullButton.IsEnabled = -not $Busy
    $modifiedRepositories = @($items | Where-Object { $_.DirtyCount -gt 0 }).Count
    $commitPushButton.IsEnabled = (-not $Busy -and $modifiedRepositories -gt 0)
    $discardButton.IsEnabled = (-not $Busy -and $null -ne $script:lastResult -and $script:lastResult.Summary.SuspiciousFiles -gt 0)
    $activityIndicator.Visibility = if ($Busy) { 'Visible' } else { 'Collapsed' }
    if ($Busy) {
        $activityIndicator.IsIndeterminate = $true
        $activityIndicator.Value = 0
    }
    else {
        $activityIndicator.IsIndeterminate = $false
        $activityIndicator.Value = 0
    }
}

function Quote-ProcessArgument {
    param([string] $Value)
    return '"' + $Value.Replace('"', '\"') + '"'
}

function Show-CommitPushDialog {
    param([Parameter(Mandatory = $true)] [object[]] $Repositories)

    $dialogXaml = @'
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Commit &amp; Push global"
    Width="570"
    Height="470"
    MinWidth="570"
    MinHeight="470"
    ResizeMode="NoResize"
    WindowStartupLocation="CenterOwner"
    Background="#0B0E14"
    Foreground="#EDF1F7"
    FontFamily="Segoe UI">
    <Grid Margin="26,22,26,22">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="16" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="12" />
            <RowDefinition Height="*" />
            <RowDefinition Height="18" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <StackPanel>
            <TextBlock Text="PUBLIER TOUS LES CHANGEMENTS" Foreground="#8F82ED" FontSize="11" FontWeight="Bold" />
            <TextBlock Text="Un message, plusieurs dépôts." FontSize="23" FontWeight="Bold" Margin="0,5,0,0" />
        </StackPanel>

        <TextBlock Grid.Row="1" Margin="0,10,0,0" Foreground="#909BAD" FontSize="12" TextWrapping="Wrap"
                   Text="Tous les fichiers modifiés seront ajoutés. Chaque dépôt recevra son propre commit avec ce même message, puis son push sera tenté." />

        <StackPanel Grid.Row="3">
            <TextBlock Text="MESSAGE DE COMMIT" Foreground="#7F8A9C" FontSize="10" FontWeight="SemiBold" />
            <TextBox x:Name="CommitMessageBox" Margin="0,7,0,0" Height="42" Padding="12,9"
                     Background="#121722" Foreground="#EDF1F7" CaretBrush="#EDF1F7"
                     BorderBrush="#3A4560" BorderThickness="1" FontSize="14" MaxLength="500" />
        </StackPanel>

        <Border Grid.Row="5" Background="#121722" BorderBrush="#283042" BorderThickness="1" CornerRadius="8" Padding="14,11">
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="8" />
                    <RowDefinition Height="*" />
                </Grid.RowDefinitions>
                <TextBlock x:Name="TargetTitle" Foreground="#AAB4C4" FontSize="11" FontWeight="SemiBold" />
                <ScrollViewer Grid.Row="2" VerticalScrollBarVisibility="Auto">
                    <TextBlock x:Name="TargetList" Foreground="#778397" FontSize="12" LineHeight="20" />
                </ScrollViewer>
            </Grid>
        </Border>

        <Grid Grid.Row="7">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="10" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBlock Text="Les erreurs éventuelles n'arrêteront pas les autres dépôts."
                       Foreground="#626D7E" FontSize="10" VerticalAlignment="Center" />
            <Button x:Name="CancelButton" Grid.Column="1" Content="Annuler" Padding="17,9"
                    Background="#1B2230" Foreground="#DDE3ED" BorderBrush="#303A4D" BorderThickness="1" />
            <Button x:Name="ConfirmButton" Grid.Column="3" Content="Commit &amp; Push" Padding="17,9" IsEnabled="False"
                    Background="#4937A8" Foreground="#F0EDFF" BorderBrush="#705CE0" BorderThickness="1" FontWeight="SemiBold" />
        </Grid>
    </Grid>
</Window>
'@

    $reader = New-Object System.Xml.XmlNodeReader ([xml] $dialogXaml)
    $dialog = [Windows.Markup.XamlReader]::Load($reader)
    $dialog.Owner = $window

    $messageBox = $dialog.FindName('CommitMessageBox')
    $targetTitle = $dialog.FindName('TargetTitle')
    $targetList = $dialog.FindName('TargetList')
    $cancelButton = $dialog.FindName('CancelButton')
    $confirmButton = $dialog.FindName('ConfirmButton')
    $repositoryCount = $Repositories.Count

    $targetTitle.Text = if ($repositoryCount -eq 1) { '1 DÉPÔT MODIFIÉ' } else { "$repositoryCount DÉPÔTS MODIFIÉS" }
    $targetList.Text = (@($Repositories | Sort-Object Name | ForEach-Object {
        $changeLabel = if ($_.DirtyCount -eq 1) { '1 changement' } else { "$($_.DirtyCount) changements" }
        "• $($_.Name)  ·  $changeLabel"
    }) -join "`n")
    $confirmButton.Content = "Commit & Push ($repositoryCount)"

    $messageBox.Add_TextChanged({
        $confirmButton.IsEnabled = -not [string]::IsNullOrWhiteSpace($messageBox.Text)
    })
    $cancelButton.Add_Click({ $dialog.Close() })
    $confirmButton.Add_Click({
        $dialog.Tag = $messageBox.Text.Trim()
        $dialog.DialogResult = $true
    })
    $dialog.Add_ContentRendered({ [void] $messageBox.Focus() })

    [void] $dialog.ShowDialog()
    if ($dialog.DialogResult -eq $true) {
        return [string] $dialog.Tag
    }
    return $null
}

function Update-LiveSummary {
    $currentItems = @($items)
    $suspiciousFiles = ($currentItems | Measure-Object -Property SuspiciousCount -Sum).Sum
    if ($null -eq $suspiciousFiles) { $suspiciousFiles = 0 }

    $totalCount.Text = if ($null -ne $script:workerProcess -and $script:progressTotal -gt 0) {
        "$($currentItems.Count) / $($script:progressTotal)"
    } else { [string] $currentItems.Count }
    $cleanCount.Text = [string] @($currentItems | Where-Object { $_.StatusKey -eq 'Clean' }).Count
    $modifiedCount.Text = [string] @($currentItems | Where-Object { $_.DirtyCount -gt 0 }).Count
    $behindCount.Text = [string] @($currentItems | Where-Object { $_.Behind -gt 0 }).Count
    $attentionCount.Text = [string] @($currentItems | Where-Object {
        $_.StatusKey -in @('Diverged', 'Error', 'NoUpstream') -or $_.SuspiciousCount -gt 0
    }).Count

    $discardButton.Visibility = if ($suspiciousFiles -gt 0) { 'Visible' } else { 'Collapsed' }
    $discardButton.Content = "↶  Discard TMP ($suspiciousFiles)"
    $discardButton.IsEnabled = ($suspiciousFiles -gt 0 -and $null -eq $script:workerProcess)

    $modifiedRepositories = @($currentItems | Where-Object { $_.DirtyCount -gt 0 }).Count
    $commitPushButton.Content = "↑  Commit & Push ($modifiedRepositories)"
    $commitPushButton.IsEnabled = ($modifiedRepositories -gt 0 -and $null -eq $script:workerProcess)

    $liveView = [System.Windows.Data.CollectionViewSource]::GetDefaultView($repositoryGrid.ItemsSource)
    $liveView.Refresh()
}

function Add-RepositoryItem {
    param($Repository)

    if ([string]::IsNullOrWhiteSpace($Repository.Operation)) {
        $Repository.Operation = '—'
    }

    if ($Repository.SuspiciousCount -gt 0) {
        $operationPrefix = if ($Repository.Operation -eq '—') { '' } else { "$($Repository.Operation) · " }
        $Repository | Add-Member -NotePropertyName OperationDisplay -NotePropertyValue ($operationPrefix + $Repository.DiagnosticText) -Force
        $Repository | Add-Member -NotePropertyName OperationColor -NotePropertyValue '#FFB45E' -Force
    }
    else {
        $Repository | Add-Member -NotePropertyName OperationDisplay -NotePropertyValue $Repository.Operation -Force
        $operationColor = if ($Repository.Operation -like '*impossible*') {
            '#FF6B7A'
        } elseif ($Repository.Operation -like 'Mis à jour*' -or $Repository.Operation -like '*restauré*' -or $Repository.Operation -eq 'Commit & push effectués') {
            '#61D6A3'
        } else { '#8E99AA' }
        $Repository | Add-Member -NotePropertyName OperationColor -NotePropertyValue $operationColor -Force
    }

    $tooltipParts = New-Object System.Collections.Generic.List[string]
    $tooltipParts.Add($Repository.Path)
    if (-not [string]::IsNullOrWhiteSpace($Repository.Remote)) { $tooltipParts.Add($Repository.Remote) }
    if (-not [string]::IsNullOrWhiteSpace($Repository.Error)) { $tooltipParts.Add($Repository.Error) }
    if ($Repository.SuspiciousCount -gt 0) {
        $tooltipParts.Add('')
        $tooltipParts.Add('CHANGEMENTS SUSPECTS')
        foreach ($finding in @($Repository.SuspiciousChanges)) {
            $tooltipParts.Add("• $($finding.Path)")
            $tooltipParts.Add("  $($finding.Reason)")
            $tooltipParts.Add("  $($finding.Advice)")
        }
    }
    $Repository | Add-Member -NotePropertyName Tooltip -NotePropertyValue ($tooltipParts -join "`n") -Force

    $existingItem = @($items | Where-Object { $_.Path -eq $Repository.Path } | Select-Object -First 1)
    if ($existingItem.Count -gt 0) {
        [void] $items.Remove($existingItem[0])
    }
    $items.Add($Repository)
    Update-LiveSummary
}

function Update-ProgressDisplay {
    if (-not (Test-Path -LiteralPath $progressFile)) { return }

    try {
        $lines = @(Get-Content -LiteralPath $progressFile -Encoding UTF8 -ErrorAction Stop | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        if ($lines.Count -le $script:progressFileLineCount) { return }

        for ($index = $script:progressFileLineCount; $index -lt $lines.Count; $index++) {
            $line = $lines[$index]

            if ($line -eq '@phase|discovery') {
                $activityIndicator.IsIndeterminate = $true
                continue
            }

            if ($line -match '^@progress\|(\d+)\|(\d+)$') {
                $current = [int] $Matches[1]
                $total = [Math]::Max(1, [int] $Matches[2])
                $script:progressTotal = $total
                $activityIndicator.IsIndeterminate = $false
                $activityIndicator.Minimum = 0
                $activityIndicator.Maximum = $total
                $activityIndicator.Value = [Math]::Min($current, $total)
                continue
            }

            if ($line.StartsWith('@repo|')) {
                $payload = $line.Substring(6)
                $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
                $repository = $json | ConvertFrom-Json
                Add-RepositoryItem -Repository $repository
                continue
            }

            $script:progressLines.Add($line)
        }

        $script:progressFileLineCount = $lines.Count

        if ($script:progressLines.Count -gt 0) {
            $statusLabel.Text = $script:progressLines[$script:progressLines.Count - 1]
            $statusLabel.ToolTip = $script:progressLines -join "`n"
        }
    }
    catch {
        # Le worker peut écrire exactement pendant la lecture ; le prochain tick réessaiera.
    }
}

function Update-RepositoryView {
    param($Result)

    $items.Clear()
    foreach ($repository in @($Result.Repositories | Sort-Object Name)) {
        Add-RepositoryItem -Repository $repository
    }

    $totalCount.Text = [string] $Result.Summary.Total
    $cleanCount.Text = [string] $Result.Summary.Clean
    $modifiedCount.Text = [string] $Result.Summary.Modified
    $behindCount.Text = [string] $Result.Summary.Behind
    $attentionCount.Text = [string] $Result.Summary.Attention
    $discardButton.Visibility = if ($Result.Summary.SuspiciousFiles -gt 0) { 'Visible' } else { 'Collapsed' }
    $discardButton.Content = "↶  Discard TMP ($($Result.Summary.SuspiciousFiles))"
    $discardButton.IsEnabled = ($Result.Summary.SuspiciousFiles -gt 0 -and $null -eq $script:workerProcess)

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

        if ($result.Mode -eq 'DiscardSuspicious') {
            $statusLabel.Text = "Restauration terminée · $($result.Summary.RestoredFiles) atlas TMP restauré(s) · $duration s · $time"
        }
        elseif ($result.Mode -eq 'CommitPush') {
            $failureSuffix = if ($result.Summary.Failed -gt 0) { " · $($result.Summary.Failed) échec(s)" } else { '' }
            $statusLabel.Text = "Publication terminée · $($result.Summary.Committed) commit(s) · $($result.Summary.Pushed) push(s)$failureSuffix · $duration s · $time"
        }
        elseif ($result.Mode -eq 'PullSafe') {
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
        if (Test-Path -LiteralPath $progressFile) {
            Remove-Item -LiteralPath $progressFile -Force -ErrorAction SilentlyContinue
        }
        if ($null -ne $script:workerProcess) {
            $script:workerProcess.Dispose()
            $script:workerProcess = $null
        }
        $script:activeMode = ''
    }
}

function Start-Refresh {
    param(
        [ValidateSet('Status', 'Fetch', 'PullSafe', 'DiscardSuspicious', 'CommitPush')] [string] $Mode,
        [string] $CommitMessage = ''
    )

    if ($null -ne $script:workerProcess -and -not $script:workerProcess.HasExited) {
        return
    }

    Remove-Item -LiteralPath $stateFile -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $progressFile -Force -ErrorAction SilentlyContinue
    $script:progressLines.Clear()
    $script:progressFileLineCount = 0
    $script:progressTotal = 0
    $statusLabel.ToolTip = $null
    $items.Clear()
    Update-LiveSummary
    $script:activeMode = $Mode
    Set-BusyState -Busy $true

    if ($Mode -eq 'DiscardSuspicious') {
        $statusLabel.Text = 'Restauration des atlas TMP confirmés…'
    }
    elseif ($Mode -eq 'CommitPush') {
        $statusLabel.Text = 'Commit et publication des dépôts modifiés…'
    }
    elseif ($Mode -eq 'PullSafe') {
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
        '-OutputPath', $stateFile,
        '-ProgressPath', $progressFile
    )

    if ($Mode -eq 'CommitPush') {
        $messageBase64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($CommitMessage))
        $arguments += @('-CommitMessageBase64', $messageBase64)
    }

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
    Update-ProgressDisplay
    if ($null -ne $script:workerProcess -and $script:workerProcess.HasExited) {
        Complete-Refresh
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
$commitPushButton.Add_Click({
    $targets = @($items | Where-Object { $_.DirtyCount -gt 0 })
    if ($targets.Count -le 0) {
        return
    }

    $commitMessage = Show-CommitPushDialog -Repositories $targets
    if (-not [string]::IsNullOrWhiteSpace($commitMessage)) {
        Start-Refresh -Mode 'CommitPush' -CommitMessage $commitMessage
    }
})
$discardButton.Add_Click({
    if ($null -eq $script:lastResult -or $script:lastResult.Summary.SuspiciousFiles -le 0) {
        return
    }

    $targets = New-Object System.Collections.Generic.List[string]
    foreach ($repository in @($script:lastResult.Repositories | Where-Object { $_.SuspiciousCount -gt 0 })) {
        foreach ($finding in @($repository.SuspiciousChanges)) {
            $targets.Add("• $($repository.Name) / $($finding.Path)")
        }
    }

    $message = @"
Git Watch va restaurer ces fichiers exactement comme dans le dernier commit :

$($targets -join "`n")

Toutes leurs modifications locales seront supprimées, y compris si elles sont stagées.
Cette action ne touche à aucun autre fichier.

Continuer ?
"@

    $choice = [System.Windows.MessageBox]::Show(
        $window,
        $message,
        'Discard des atlas TMP',
        [System.Windows.MessageBoxButton]::YesNo,
        [System.Windows.MessageBoxImage]::Warning,
        [System.Windows.MessageBoxResult]::No
    )

    if ($choice -eq [System.Windows.MessageBoxResult]::Yes) {
        Start-Refresh -Mode 'DiscardSuspicious'
    }
})

$repositoryGrid.Add_MouseDoubleClick({
    if ($null -ne $repositoryGrid.SelectedItem) {
        Start-Process -FilePath 'explorer.exe' -ArgumentList $repositoryGrid.SelectedItem.Path
    }
})

$window.Add_Loaded({
    if ([string]::IsNullOrWhiteSpace($PreviewPath)) {
        $pollTimer.Start()
        Start-Refresh -Mode 'Fetch'
    }
})

$window.Add_Closed({
    $pollTimer.Stop()
    if ($null -ne $script:workerProcess -and -not $script:workerProcess.HasExited) {
        try { $script:workerProcess.Kill() } catch { }
    }
    Remove-Item -LiteralPath $stateFile -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $progressFile -Force -ErrorAction SilentlyContinue
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

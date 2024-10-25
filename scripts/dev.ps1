Add-Type -AssemblyName PresentationFramework

$auto = New-Object System.Windows.GridLength(1, [System.Windows.GridUnitType]::Auto)
$star1 = New-Object System.Windows.GridLength(1, [System.Windows.GridUnitType]::Star)
$star2 = New-Object System.Windows.GridLength(2, [System.Windows.GridUnitType]::Star)

function Add-SecondaryButton($parent, $content, $margin){
    $button = New-Object System.Windows.Controls.Button
    $button.Content = $content
    $button.Margin = $margin
    $button.Style = $window.FindResource("SecondaryButton")
    if($null -ne $parent){
        $parent.AddChild($button)
    }
    return $button
}

function Add-runButton($parent, $content, $margin){
    $button = New-Object System.Windows.Controls.Button
    $button.Content = $content
    $button.Margin = $margin
    $button.Style = $window.FindResource("PrimaryButton")
    if($null -ne $parent){
        $parent.AddChild($button)
    }
    return $button
}

<#
.SYNOPSIS
    テキストブロックの作成
.DESCRIPTION
    テキストブロックを作成し、親ノードに追加
.PARAMETER parent
    親ノード
.PARAMETER txt
    テキスト
.PARAMETER margin
    余白 (margin)
.EXAMPLE
    Add-TextBlock $mainPanel "テキスト" "8, 8, 8, 8"
#>
function Add-TextBlock($parent, $txt, $margin){
    $text = New-Object System.Windows.Controls.TextBlock
    $text.Text = $txt
    $text.FontSize = "16"
    $text.Margin = $margin
    if($null -ne $parent){
        $parent.AddChild($text)
    }
    return $text
}

function Add-TextBox($parent, $txt, $margin){
    $text = New-Object System.Windows.Controls.TextBox
    $text.Text = $txt
    $text.Style = $window.FindResource("TextBox")
    $text.Margin = $margin
    if($null -ne $parent){
        $parent.AddChild($text)
    }
    return $text
}

function Add-RowGrid($parent, $gridLengths, $controls){
    $grid = New-Object System.Windows.Controls.Grid
    for($i = 0; $i -lt $gridLengths.Count; $i++){
        $row = New-Object System.Windows.Controls.RowDefinition
        $grid.RowDefinitions.Add($row)
        $row.Height = $gridLengths[$i]
        [System.Windows.Controls.Grid]::SetRow($controls[$i], $i)
        $grid.Children.Add($controls[$i])
    }
    if($null -ne $parent){
        $parent.AddChild($grid)
    }
    return $grid
}

function Add-ColumnGrid($parent, $gridLengths, $controls){
    $grid = New-Object System.Windows.Controls.Grid
    for($i = 0; $i -lt $gridLengths.Count; $i++){
        $column = New-Object System.Windows.Controls.ColumnDefinition
        $grid.ColumnDefinitions.Add($column)
        $column.Width = $gridLengths[$i]
        [System.Windows.Controls.Grid]::SetColumn($controls[$i], $i)
        $grid.Children.Add($controls[$i])
    }
    if($null -ne $parent){
        $parent.AddChild($grid)
    }
    return $grid
}

$pjFile = Get-ChildItem -Path . -Recurse -Name "*.csproj"
if(-not ($pjFile -is [array])){
    if(Test-Path $pjFile){
        $projectFile = $pjFile
    }
}

[xml]$xaml = Get-Content ".\scripts\dev.xaml"
$reader = New-Object System.Xml.XmlNodeReader $xaml
$window = [Windows.Markup.XamlReader]::Load($reader)
$window.Title = "開発支援ツール"
$window.Height = "130"
$window.Width = "440"
$mainPanel = $window.FindName("MainPanel")

$appNameLabel = Add-TextBlock $null "プロジェクトの場所" "8, 12, 0, 8"
$appNameBox = Add-TextBox $null $projectFile "12, 8, 8, 0"
$appNameGrid = Add-ColumnGrid $mainPanel @($auto, $star1) @($appNameLabel, $appNameBox)

$runButton = Add-runButton $null "実行" "8, 8, 4, 8"
$publishButton = Add-SecondaryButton $null "発行" "4, 8, 8, 8"
$gridButtons = Add-ColumnGrid $mainPanel @($star1, $star1) @($runButton, $publishButton)

# 実行をクリックしたとき
$runButton.Add_Click({
    if(-not (Test-Path $appNameBox.Text)){
        [System.Windows.MessageBox]::Show("プロジェクトが存在しません。", "開発支援ツール", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Error)
        return
    }
    $currentDir = $(Convert-Path .)
    $job = Start-Job -ScriptBlock {
        param (
            [string]$currentDir,
            [string]$projectFile
        )
        dotnet run start --project $currentDir\$projectFile
    } -ArgumentList $currentDir, $appNameBox.Text
    [System.Windows.MessageBox]::Show("実行中です。起動までしばらくお待ちください。", "開発支援ツール", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Information)
})

# 発行をクリックしたとき
$publishButton.Add_Click({
    if(-not (Test-Path $appNameBox.Text)){
        [System.Windows.MessageBox]::Show("プロジェクトが存在しません。", "開発支援ツール", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Error)
        return
    }
    $currentDir = $(Convert-Path .)
    $job = Start-Job -ScriptBlock {
        param (
            [string]$currentDir,
            [string]$projectFile
        )
        dotnet publish $currentDir\$projectFile  -c Release /p:Version=23.09.23
    } -ArgumentList $currentDir, $appNameBox.Text
    [System.Windows.MessageBox]::Show("発行中です。終了までしばらくお待ちください。", "開発支援ツール", [System.Windows.MessageBoxButton]::OK, [System.Windows.MessageBoxImage]::Information)
})

$window.ShowDialog() | Out-Null

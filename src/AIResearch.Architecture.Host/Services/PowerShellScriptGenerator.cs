namespace AIResearch.Architecture.Host.Services;

public class PowerShellScriptGenerator(BaseUrlResolver baseUrlResolver) : IPowerShellScriptGenerator
{
    public string GenerateHelpers()
    {
        var architectureBaseUrl = baseUrlResolver.GetArchitectureBaseUrl();

        var script = $@"
# =============================================================================
# IaResearch Architecture API - PowerShell Helper Functions
# Copy this entire block and paste into your PowerShell session
# =============================================================================

$script:ArchBaseUrl = '{architectureBaseUrl}'

$script:SafeRestMethod = {{
    param(
        [string]$Uri,
        [string]$Method = 'GET',
        [string]$ContentType,
        [string]$Body
    )
    try {{
        $params = @{{ Uri = $Uri; Method = $Method }}
        if ($ContentType) {{ $params.ContentType = $ContentType }}
        if ($Body) {{ $params.Body = $Body }}
        Invoke-RestMethod @params
    }} catch {{
        $errorBody = ''
        try {{
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $reader.BaseStream.Position = 0
            $reader.DiscardBufferedData()
            $errorBody = $reader.ReadToEnd()
        }} catch {{ }}
        throw ""HTTP $($_.Exception.Response.StatusCode): $errorBody""
    }}
}}

function Invoke-ArchGetArchitecture {{
    Write-Host 'Invoke-ArchGetArchitecture' -ForegroundColor Magenta
    & $script:SafeRestMethod -Uri ""$script:ArchBaseUrl/get-architecture"" -Method GET
}}

function Invoke-ArchGetRules {{
    Write-Host 'Invoke-ArchGetRules' -ForegroundColor Magenta
    & $script:SafeRestMethod -Uri ""$script:ArchBaseUrl/rules"" -Method GET
}}

function Invoke-ArchGetComponentRoles {{
    Write-Host 'Invoke-ArchGetComponentRoles' -ForegroundColor Magenta
    & $script:SafeRestMethod -Uri ""$script:ArchBaseUrl/component-roles"" -Method GET
}}

function Invoke-ArchGenerateWorkspace {{
    param(
        [Parameter(Mandatory)][string]$SolutionName,
        [Parameter(Mandatory)][array]$Features,
        [string]$ArchitectureStyle = 'CleanArchitecture',
        [string]$TargetFramework = 'net9.0'
    )
    Write-Host 'Invoke-ArchGenerateWorkspace' -ForegroundColor Magenta
    $body = @{{
        SolutionName = $SolutionName
        Features = $Features
        ArchitectureStyle = $ArchitectureStyle
        TargetFramework = $TargetFramework
    }} | ConvertTo-Json -Depth 10 -Compress
    & $script:SafeRestMethod -Uri ""$script:ArchBaseUrl/generate-workspace"" -Method POST -ContentType 'application/json' -Body $body
}}

function Invoke-ArchGenerate {{
    param(
        [Parameter(Mandatory)][string]$SolutionName,
        [Parameter(Mandatory)][array]$Features,
        [Parameter(Mandatory)][array]$Components
    )
    Write-Host 'Invoke-ArchGenerate' -ForegroundColor Magenta
    $body = @{{
        SolutionName = $SolutionName
        Features = $Features
        Components = $Components
    }} | ConvertTo-Json -Depth 10 -Compress
    & $script:SafeRestMethod -Uri ""$script:ArchBaseUrl/generate"" -Method POST -ContentType 'application/json' -Body $body
}}

function Invoke-ArchValidate {{
param(
    [Parameter(HelpMessage='Pass $result.files directly. Accepts PSCustomObject (path=key/content=value), hashtable, or array of @{{Path=...; code=...}}. Do NOT transform or iterate $result.files before passing.')]$Files
)
Write-Host 'Invoke-ArchValidate' -ForegroundColor Magenta

    if ($null -eq $Files) {{
        throw @'
Invoke-ArchValidate: $Files is $null - no files were saved.
The previous MCP request most likely failed and must be fixed before retrying.
Check the response of the failed request for error details (e.g. validation errors, missing parameters, or server-side exceptions).
'@
    }}

    $normalizedFiles = @{{}}

    # Handle PSCustomObject (from Invoke-RestMethod JSON deserialization of object)
    if ($Files -is [System.Management.Automation.PSCustomObject]) {{
        $Files.PSObject.Properties | ForEach-Object {{ $normalizedFiles[$_.Name] = $_.Value }}
    }}
    # Handle array of file objects (each with Path and code properties)
    elseif ($Files -is [System.Array]) {{
        foreach ($file in $Files) {{
            if ($file.Path -and $file.code) {{
                $normalizedFiles[$file.Path] = $file.code
            }}
        }}
    }}
    # Handle hashtable
    elseif ($Files -is [hashtable]) {{
        $normalizedFiles = $Files
    }}
    else {{
        throw ""Unsupported type for Files parameter: $($Files.GetType().FullName). It must be either a [System.Management.Automation.PSCustomObject], [System.Array] or a [hashtable]""
    }}

    $body = $normalizedFiles | ConvertTo-Json -Depth 10 -Compress
    & $script:SafeRestMethod -Uri ""$script:ArchBaseUrl/validate"" -Method POST -ContentType 'application/json' -Body $body
}}

function Read-SourceFile {{
    param([Parameter(Mandatory)][string]$Path)
    Write-Host 'Read-SourceFile' -ForegroundColor Magenta
    [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
}}

function Read-SourceFiles {{
    param(
        [Parameter(Mandatory)][string]$Directory,
        [string]$Filter = '*.cs',
        [switch]$Recurse
    )
    Write-Host 'Read-SourceFiles' -ForegroundColor Magenta
    $files = @{{}}
    $params = @{{ Path = $Directory; Filter = $Filter }}
    if ($Recurse) {{ $params.Recurse = $true }}
    Get-ChildItem @params | ForEach-Object {{
        $relativePath = $_.FullName.Replace((Resolve-Path $Directory).Path, '').TrimStart('\', '/')
        $files[$relativePath] = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8)
    }}
    $files
}}

function Save-GeneratedFiles {{
param(
    [Parameter(HelpMessage='Pass $result.files directly. Accepts PSCustomObject (path=key/content=value), hashtable, or array of @{{Path=...; code=...}}. Do NOT transform or iterate $result.files before passing.')]$Files
)
Write-Host 'Save-GeneratedFiles' -ForegroundColor Magenta

    if ($null -eq $Files) {{
        throw @'
Save-GeneratedFiles: $Files is $null - no files were saved.
The previous MCP request most likely failed and must be fixed before retrying.
Check the response of the failed request for error details (e.g. validation errors, missing parameters, or server-side exceptions).
'@
    }}

    # Handle PSCustomObject (from Invoke-RestMethod JSON deserialization of object)
    if ($Files -is [System.Management.Automation.PSCustomObject]) {{
        $Files.PSObject.Properties | ForEach-Object {{
            $fullPath = $_.Name
            $content = $_.Value
            $dir = Split-Path $fullPath -Parent
            if ($dir -and -not (Test-Path $dir)) {{ New-Item -ItemType Directory -Path $dir -Force | Out-Null }}
            [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.Encoding]::UTF8)
            Write-Host ""Created: $fullPath""
        }}
    }}
    # Handle array of file objects (each with Path and code properties)
    elseif ($Files -is [System.Array]) {{
        foreach ($file in $Files) {{
            $fullPath = $file.Path
            $content = $file.code
            if (-not $content) {{
                Write-Warning ""Skipping $fullPath - no content found (expected 'code' property)""
                continue
            }}
            $dir = Split-Path $fullPath -Parent
            if ($dir -and -not (Test-Path $dir)) {{ New-Item -ItemType Directory -Path $dir -Force | Out-Null }}
            [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.Encoding]::UTF8)
            Write-Host ""Created: $fullPath""
        }}
    }}
    # Handle hashtable
    elseif ($Files -is [hashtable]) {{
        foreach ($entry in $Files.GetEnumerator()) {{
            $fullPath = $entry.Key
            $dir = Split-Path $fullPath -Parent
            if ($dir -and -not (Test-Path $dir)) {{ New-Item -ItemType Directory -Path $dir -Force | Out-Null }}
            [System.IO.File]::WriteAllText($fullPath, $entry.Value, [System.Text.Encoding]::UTF8)
            Write-Host ""Created: $fullPath""
        }}
    }}
    else {{
        throw ""Unsupported type for Files parameter: $($Files.GetType().FullName). It must be either a [System.Management.Automation.PSCustomObject], [System.Array] or a [hashtable]""
    }}
}}

Write-Host '? IaResearch Architecture API helpers loaded.' -ForegroundColor Green
Write-Host ''
Write-Host 'Available commands:' -ForegroundColor Cyan
Write-Host '  Invoke-ArchGetArchitecture        - Get architecture definition'
Write-Host '  Invoke-ArchGetRules               - Get architecture rules'
Write-Host '  Invoke-ArchGetComponentRoles      - Get all valid component roles (use for ComponentRole values)'
Write-Host '  Invoke-ArchGenerateWorkspace      - Generate solution structure'
Write-Host '  Invoke-ArchGenerate               - Generate components'
Write-Host '  Invoke-ArchValidate               - Validate source files'
Write-Host '  Read-SourceFile                   - Read a single file (safe)'
Write-Host '  Read-SourceFiles                  - Read multiple files to hashtable'
Write-Host '  Save-GeneratedFiles               - Save generated files to src folder'
Write-Host ''
Write-Host 'Example usage:' -ForegroundColor Yellow
Write-Host ''
Write-Host '  IMPORTANT: $result.files is a PSCustomObject where each property name is a file' -ForegroundColor Yellow
Write-Host '  path and the value is the file content. Always pass it directly to Save-GeneratedFiles' -ForegroundColor Yellow
Write-Host '  and Invoke-ArchValidate. Do NOT iterate, transform, or convert it first.' -ForegroundColor Yellow
Write-Host ''
Write-Host '  # Generate workspace'
Write-Host '  $result = Invoke-ArchGenerateWorkspace -SolutionName ""MyApp"" -Features @(@{{Name=""Order"";ApplicationKind=""WebApi""}})'
Write-Host '  Save-GeneratedFiles -Files $result.files'
Write-Host ''
Write-Host '  # Get valid component roles before generating (ComponentRole values MUST come from this list)'
Write-Host '  $roles = Invoke-ArchGetComponentRoles'
Write-Host '  $roles.componentRoles | Select-Object name, layer, typeKind | Format-Table'
Write-Host ''
Write-Host '  # Generate components (NOTE: Use ComponentRole, not Role)'
Write-Host '  $components = @('
Write-Host '      @{{ComponentRole=""DomainModel""; Name=""Order""}}'
Write-Host '      @{{ComponentRole=""Command""; Name=""CreateOrderCommand""}}'
Write-Host '      @{{ComponentRole=""CommandHandler""; Name=""CreateOrderCommandHandler""}}'
Write-Host '  )'
Write-Host '  $result = Invoke-ArchGenerate -SolutionName ""MyApp"" -Features @(@{{Name=""Order""}}) -Components $components'
Write-Host '  Save-GeneratedFiles -Files $result.files'
Write-Host ''
Write-Host '  # Validate after generating: pass $result.files directly - no transformation needed'
Write-Host '  Invoke-ArchValidate -Files $result.files'
Write-Host ''
Write-Host '  # Validate files from disk'
Write-Host '  $files = Read-SourceFiles -Directory "".\src"" -Recurse'
Write-Host '  Invoke-ArchValidate -Files $files'
";
        return script.ReplaceLineEndings("\n");
    }
}

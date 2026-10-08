# Test data for 1012 format 13 (1480Dual txt E-data) in OUR OWN test DB 1480dual_test_tanawut
# (no MES trigger there). Writes the folder tree under \\office_SRV\C06_Shipping-Data\Test\1480DUAL and the
# matching SQL (db_check_sheet + shipping). txt content is synthetic, same layout as the real files.
param([string]$Root = '\\office_SRV\C06_Shipping-Data\Test\1480DUAL', [string]$SqlOut,
      [string]$Db = '1480dual_test_tanawut')

$nameInEmail = '48Dual'
$partNo = 'FRL1441D5050-878-1427-1457'   # placeholder until the engineer confirms the Part_No of 901 / 902
$note = 'TEST 1012 format13'
$cust = @{
    COHERENT = @{ key = 901; spec = 'Y-3ER4003A' }
    INFINERA = @{ key = 902; spec = 'Y-3ER2002A' }
}

# lines = D lines per LD: 8000 ~ 693 KB (like the real file), 20 ~ 2 KB (for the 256-file test)
$orders = @(
    @{ d='2026-09-21'; c='COHERENT'; fec='3099990101-10-001'; tr='TST13-0101'; sp='TSTA'; db=3;   txt=3;   lines=8000; mode='ok' }
    @{ d='2026-09-21'; c='COHERENT'; fec='3099990102-10-001'; tr='TST13-0102'; sp='TSTB'; db=2;   txt=2;   lines=8000; mode='ok' }
    @{ d='2026-09-21'; c='INFINERA'; fec='3099990111-10-001'; tr='TST13-0111'; sp='TSTH'; db=4;   txt=4;   lines=8000; mode='ok' }
    @{ d='2026-09-22'; c='COHERENT'; fec='3099990201-10-001'; tr='TST13-0201'; sp='TSTC'; db=20;  txt=20;  lines=8000; mode='ok' }
    @{ d='2026-09-22'; c='COHERENT'; fec='3099990202-10-001'; tr='TST13-0202'; sp='TSTD'; db=10;  txt=10;  lines=8000; mode='ok' }
    @{ d='2026-09-22'; c='INFINERA'; fec='3099990211-10-001'; tr='TST13-0211'; sp='TSTI'; db=16;  txt=16;  lines=8000; mode='ok' }
    @{ d='2026-09-23'; c='COHERENT'; fec='3099990301-10-001'; tr='TST13-0301'; sp='TSTE'; db=2;   txt=0;   lines=8000; mode='noEdata' }
    @{ d='2026-09-24'; c='COHERENT'; fec='3099990401-10-001'; tr='TST13-0401'; sp='TSTF'; db=3;   txt=2;   lines=8000; mode='ok' }
    @{ d='2026-09-25'; c='COHERENT'; fec='3099990501-10-001'; tr='TST13-0501'; sp='TSTG'; db=2;   txt=0;   lines=8000; mode='noFolder' }
    @{ d='2026-09-26'; c='INFINERA'; fec='3099990611-10-001'; tr='TST13-0611'; sp='TSTK'; db=300; txt=300; lines=20;   mode='ok' }
)

function New-TxtEdata([string]$path, [string]$serial, [string]$shipDate, [int]$lines) {
    $rnd = New-Object System.Random ([Math]::Abs($serial.GetHashCode()))
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('Q,1')
    [void]$sb.AppendLine("H,$serial-1,N/A,N/A,,$partNo,$shipDate,1,286.5,1994.8,2234.1,500,555.4,1.161,1426.7,4.3,1.84,1.227,1426.6,0.4,24.7")
    [void]$sb.AppendLine("H,$serial-2,N/A,N/A,,$partNo,$shipDate,2,341.4,2004.3,2244.9,500,561.4,1.498,1456.7,4.3,1.79,1.178,1456.6,0.4,19.9")
    foreach ($ld in 1, 2) {
        for ($i = 0; $i -lt $lines; $i++) {
            $a = [Math]::Round($rnd.NextDouble(), 2); $b = [Math]::Round(0.7 + $rnd.NextDouble() * 0.1, 3)
            $c = [Math]::Round(($rnd.NextDouble() - 0.5) * 0.02, 3)
            [void]$sb.AppendLine(("D,{0}-{1},25,{2},{3:0.00},{4:0.000},{5:0.000},0.000" -f $serial, $ld, $i, $a, $b, $c))
        }
    }
    [System.IO.File]::WriteAllText($path, $sb.ToString(), (New-Object System.Text.ASCIIEncoding))
}

$sql = New-Object System.Collections.Generic.List[string]
$sql.Add("-- test rows for 1012 format 13 in $Db, re-runnable (removes its own rows first)")
$sql.Add("DELETE FROM ``$Db``.shipping WHERE Note = '$note';")
$sql.Add("DELETE FROM ``$Db``.db_check_sheet WHERE TestReportNo LIKE 'TST13-%';")

foreach ($o in $orders) {
    $key = $cust[$o.c].key; $spec = $cust[$o.c].spec
    $date = [datetime]::ParseExact($o.d, 'yyyy-MM-dd', $null)
    $yymmdd = $date.ToString('yyMMdd'); $hdrDate = $date.ToString('dd-MMM-yy', [Globalization.CultureInfo]::InvariantCulture)
    $customerFolder = Join-Path $Root "$yymmdd-Shipping Data\$yymmdd-$($o.c)($nameInEmail)"
    New-Item -ItemType Directory -Force -Path $customerFolder | Out-Null

    $orderName = "$yymmdd-$($o.fec)($partNo)"
    if ($o.mode -ne 'noFolder') {
        $orderFolder = Join-Path $customerFolder "$orderName-1"
        New-Item -ItemType Directory -Force -Path (Join-Path $orderFolder 'For QA check') | Out-Null
        if ($o.mode -eq 'ok') {
            $edata = Join-Path $orderFolder "${orderName}_Edata"
            New-Item -ItemType Directory -Force -Path $edata | Out-Null
            for ($n = 1; $n -le $o.txt; $n++) {
                $serial = '{0}{1:000}' -f $o.sp, $n
                New-TxtEdata (Join-Path $edata "$serial.txt") $serial $hdrDate $o.lines
            }
        }
    }

    $sql.Add("INSERT INTO ``$Db``.db_check_sheet (TestReportNo, SubFolder, FECOrderNo, Customer, Spec, PartNo, ShipDate, ShipQty, PrintDate, OperatorID, CheckSheet, Manual, ManualNo, Inuse) VALUES ('$($o.tr)', 1, '$($o.fec)', '$($o.c)', '$spec', '$partNo', '$($o.d)', $($o.db), NOW(), 'TEST', 1, 0, '', 1);")
    $rows = @()
    for ($n = 1; $n -le $o.db; $n++) {
        $serial = '{0}{1:000}' -f $o.sp, $n
        $rows += "('$($o.d)', '$serial', 'TEST', NOW(), $key, '$($o.c)', '$partNo', '$spec', '$($o.fec)', '$($o.tr)', '$note')"
    }
    $sql.Add("INSERT INTO ``$Db``.shipping (Ship_Date, Serial, SubLot, InsertTime, CusCond_ID, Customer, PartNo, Spec, FECOrderNo, TestReportNo, Note) VALUES " + ($rows -join ', ') + ";")
}

Set-Content -Path $SqlOut -Value $sql -Encoding UTF8
"done: $SqlOut"

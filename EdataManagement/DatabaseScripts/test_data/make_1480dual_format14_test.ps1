# Test data for 1012 format 14 (1480Dual MOLEX txt -> zip) in OUR OWN test DB 1480dual_test_tanawut.
# Folder tree under \\office_SRV\C06_Shipping-Data\Test\1480DUAL + matching SQL (db_check_sheet + shipping).
# One synthetic txt per order in the real layout (2 H lines + ~8000 D lines per LD per serial, ~695 KB per serial).
param([string]$Root = '\\office_SRV\C06_Shipping-Data\Test\1480DUAL', [string]$SqlOut,
      [string]$Db = '1480dual_test_tanawut')

$customer = 'MOLEX'; $nameInEmail = '48Dual'; $keyId = 7; $spec = 'Y-3ER4002C'
$partNo = 'FRL1441D4045-878-1427-1457'; $customerPN = 'NA'   # 1480dual.info_cus_condition KEY 7
$mpn = $partNo.Split('-')[0]
$note = 'TEST 1012 format14'

# qty = serials in DB; txtQty = the PCS written in the txt name (differs only for the wrong-name case)
$orders = @(
    @{ d='2026-09-15'; fec='3099991501-10-001'; tr='TST14-1501'; sp='TMXA'; qty=10; txtQty=10; mode='ok' }
    @{ d='2026-09-16'; fec='3099991601-10-001'; tr='TST14-1601'; sp='TMXB'; qty=10; txtQty=10; mode='ok' }
    @{ d='2026-09-16'; fec='3099991602-10-001'; tr='TST14-1602'; sp='TMXC'; qty=10; txtQty=10; mode='ok' }
    @{ d='2026-09-16'; fec='3099991603-10-001'; tr='TST14-1603'; sp='TMXD'; qty=3;  txtQty=3;  mode='ok' }
    @{ d='2026-09-17'; fec='3099991701-10-001'; tr='TST14-1701'; sp='TMXE'; qty=10; txtQty=9;  mode='ok' }
    @{ d='2026-09-18'; fec='3099991801-10-001'; tr='TST14-1801'; sp='TMXF'; qty=2;  txtQty=2;  mode='ok' }
    @{ d='2026-09-18'; fec='3099991802-10-001'; tr='';           sp='TMXG'; qty=0;  txtQty=2;  mode='folderOnly' }
    @{ d='2026-09-19'; fec='3099991901-10-001'; tr='TST14-1901'; sp='TMXH'; qty=2;  txtQty=2;  mode='noFolder' }
)

function New-MolexTxt([string]$path, [string[]]$serials, [string]$hdrDate) {
    $rnd = New-Object System.Random ([Math]::Abs($path.GetHashCode()))
    $sb = New-Object System.Text.StringBuilder
    foreach ($s in $serials) {
        [void]$sb.AppendLine("H,$s-1,NA,,$hdrDate 00:00,1,291,1642.1,1839.2,400,450.4,0.967,1426.5,4343,1.97,1.3,NA,1649.6,1847.6,NA,NA,1.61,1.59,NA")
        [void]$sb.AppendLine("H,$s-2,NA,,$hdrDate 00:00,2,419,2123.5,2378.3,450,511.5,1.435,1456.6,4347,2.09,1.4,NA,2122.7,2377.5,NA,NA,1.46,1.44,NA")
    }
    foreach ($s in $serials) {
        foreach ($ld in 1, 2) {
            for ($i = 0; $i -lt 8002; $i++) {
                [void]$sb.AppendLine(("D,{0}-{1},25,{2},{3:0.0},{4:0.00},NA,0.000" -f $s, $ld, $i, [Math]::Round($rnd.NextDouble(), 1), (0.7 + $rnd.NextDouble() * 0.1)))
            }
        }
    }
    [System.IO.File]::WriteAllText($path, $sb.ToString(), (New-Object System.Text.ASCIIEncoding))
}

$sql = New-Object System.Collections.Generic.List[string]
$sql.Add("-- test rows for 1012 format 14 in $Db, re-runnable (removes its own rows first)")
$sql.Add("DELETE FROM ``$Db``.shipping WHERE Note = '$note';")
$sql.Add("DELETE FROM ``$Db``.db_check_sheet WHERE TestReportNo LIKE 'TST14-%';")

foreach ($o in $orders) {
    $date = [datetime]::ParseExact($o.d, 'yyyy-MM-dd', $null)
    $yymmdd = $date.ToString('yyMMdd'); $hdrDate = $date.ToString('MM/dd/yyyy', [Globalization.CultureInfo]::InvariantCulture)
    $customerFolder = Join-Path $Root "$yymmdd-Shipping Data\$yymmdd-$customer($nameInEmail)"
    New-Item -ItemType Directory -Force -Path $customerFolder | Out-Null
    $serials = @(for ($n = 1; $n -le [Math]::Max($o.qty, $o.txtQty); $n++) { '{0}{1:000}' -f $o.sp, $n })

    if ($o.mode -ne 'noFolder') {
        $orderName = "$yymmdd-$($o.fec)($partNo)"
        $orderFolder = Join-Path $customerFolder "$orderName-1"
        New-Item -ItemType Directory -Force -Path (Join-Path $orderFolder 'For QA check') | Out-Null
        # the xls next to the txt must NOT end up in the zip
        Set-Content -Path (Join-Path $orderFolder "$orderName.xls") -Value 'TEST placeholder - not a real xls' -Encoding ASCII
        $txtName = '{0}_{1}_{2}PCS_{3}_{4}-1.txt' -f $customerPN, $o.fec.Replace('-', ''), $o.txtQty, $mpn, $yymmdd
        New-MolexTxt (Join-Path $orderFolder $txtName) $serials[0..($o.txtQty - 1)] $hdrDate
    }

    if ($o.mode -eq 'folderOnly') { continue }   # a test report folder the database does not know
    $sql.Add("INSERT INTO ``$Db``.db_check_sheet (TestReportNo, SubFolder, FECOrderNo, Customer, Spec, PartNo, ShipDate, ShipQty, PrintDate, OperatorID, CheckSheet, Manual, ManualNo, Inuse) VALUES ('$($o.tr)', 1, '$($o.fec)', '$customer', '$spec', '$partNo', '$($o.d)', $($o.qty), NOW(), 'TEST', 1, 0, '', 1);")
    $rows = @()
    for ($n = 0; $n -lt $o.qty; $n++) {
        $rows += "('$($o.d)', '$($serials[$n])', 'TEST', NOW(), $keyId, '$customer', '$partNo', '$spec', '$($o.fec)', '$($o.tr)', '$note')"
    }
    $sql.Add("INSERT INTO ``$Db``.shipping (Ship_Date, Serial, SubLot, InsertTime, CusCond_ID, Customer, PartNo, Spec, FECOrderNo, TestReportNo, Note) VALUES " + ($rows -join ', ') + ";")
}

Set-Content -Path $SqlOut -Value $sql -Encoding UTF8
"done: $SqlOut"

# Mail-split test data for 1012 format 14 (1480Dual MOLEX txt -> zip) in OUR OWN test DB 1480dual_test_tanawut.
#   260927: 3 orders whose txt hardly compress (~12 MB txt -> ~4 MB zip each) -> 3 zips, mail (1/2) = 2 zips, mail (2/2) = 1 zip
#   260512: DB rows for the orders 10-002..10-004 the user added on the share (txt of exactly 10 MiB each, made by the user,
#           not touched here) -> 4 zips in one mail
# Writes the 260927 folders + an SQL file (db_check_sheet + shipping); re-runnable, removes its own rows first.
# (no [Parameter()] here: it would add -Debug, whose alias -db clashes with $Db)
param([string]$Root = '\\192.168.14.35\C06_Shipping-Data\Test\1480DUAL', [string]$SqlOut,
      [string]$Db = '1480dual_test_tanawut')
if (-not $SqlOut) { throw 'give -SqlOut <file.sql>' }

$customer = 'MOLEX'; $nameInEmail = '48Dual'; $keyId = 7; $spec = 'Y-3ER4002C'
$partNo = 'FRL1441D4045-878-1427-1457'; $customerPN = 'NA'   # 1480dual.info_cus_condition KEY 7
$mpn = $partNo.Split('-')[0]
$note = 'TEST 1012 format14 split'

# make = write the folder + txt here; existing = the folder + txt must already be on the share
$orders = @(
    @{ d='2026-09-27'; fec='3099992701-10-001'; tr='TST14S-2701';  sp='TMXJ'; qty=10; make=$true }
    @{ d='2026-09-27'; fec='3099992702-10-001'; tr='TST14S-2702';  sp='TMXK'; qty=10; make=$true }
    @{ d='2026-09-27'; fec='3099992703-10-001'; tr='TST14S-2703';  sp='TMXL'; qty=10; make=$true }
    @{ d='2026-05-12'; fec='3012359006-10-002'; tr='TST14S-0512B'; sp='TMXM'; qty=10; make=$false }
    @{ d='2026-05-12'; fec='3012359006-10-003'; tr='TST14S-0512C'; sp='TMXN'; qty=10; make=$false }
    @{ d='2026-05-12'; fec='3012359006-10-004'; tr='TST14S-0512D'; sp='TMXP'; qty=10; make=$false }
)

# Same layout as make_1480dual_format14_test.ps1, but six random 6-decimal values per D line so the zip stays big
function New-DenseMolexTxt([string]$path, [string[]]$serials, [string]$hdrDate) {
    $rnd = New-Object System.Random ([Math]::Abs($path.GetHashCode()))
    $inv = [Globalization.CultureInfo]::InvariantCulture
    $sb = New-Object System.Text.StringBuilder
    foreach ($s in $serials) {
        [void]$sb.AppendLine("H,$s-1,NA,,$hdrDate 00:00,1,291,1642.1,1839.2,400,450.4,0.967,1426.5,4343,1.97,1.3,NA,1649.6,1847.6,NA,NA,1.61,1.59,NA")
        [void]$sb.AppendLine("H,$s-2,NA,,$hdrDate 00:00,2,419,2123.5,2378.3,450,511.5,1.435,1456.6,4347,2.09,1.4,NA,2122.7,2377.5,NA,NA,1.46,1.44,NA")
    }
    foreach ($s in $serials) {
        foreach ($ld in 1, 2) {
            for ($i = 0; $i -lt 8002; $i++) {
                [void]$sb.Append("D,$s-$ld,25,$i")
                for ($v = 0; $v -lt 6; $v++) { [void]$sb.Append(',').Append($rnd.NextDouble().ToString('0.000000', $inv)) }
                [void]$sb.AppendLine()
            }
        }
    }
    [System.IO.File]::WriteAllText($path, $sb.ToString(), (New-Object System.Text.ASCIIEncoding))
}

$sql = New-Object System.Collections.Generic.List[string]
$sql.Add("-- mail-split test rows for 1012 format 14 in $Db, re-runnable (removes its own rows first)")
$sql.Add("DELETE FROM ``$Db``.shipping WHERE Note = '$note';")
$sql.Add("DELETE FROM ``$Db``.db_check_sheet WHERE TestReportNo LIKE 'TST14S-%';")

foreach ($o in $orders) {
    $date = [datetime]::ParseExact($o.d, 'yyyy-MM-dd', $null)
    $yymmdd = $date.ToString('yyMMdd'); $hdrDate = $date.ToString('MM/dd/yyyy', [Globalization.CultureInfo]::InvariantCulture)
    $customerFolder = Join-Path $Root "$yymmdd-Shipping Data\$yymmdd-$customer($nameInEmail)"
    $serials = @(for ($n = 1; $n -le $o.qty; $n++) { '{0}{1:000}' -f $o.sp, $n })
    $orderName = "$yymmdd-$($o.fec)($partNo)"
    $orderFolder = Join-Path $customerFolder "$orderName-1"
    $txtPath = Join-Path $orderFolder ('{0}_{1}_{2}PCS_{3}_{4}-1.txt' -f $customerPN, $o.fec.Replace('-', ''), $o.qty, $mpn, $yymmdd)

    if ($o.make) {
        New-Item -ItemType Directory -Force -Path (Join-Path $orderFolder 'For QA check') | Out-Null
        # the xls next to the txt must NOT end up in the zip
        Set-Content -LiteralPath (Join-Path $orderFolder "$orderName.xls") -Value 'TEST placeholder - not a real xls' -Encoding ASCII
        New-DenseMolexTxt $txtPath $serials $hdrDate
    }
    elseif (-not (Test-Path -LiteralPath $txtPath)) {
        throw "missing on the share (expected to exist): $txtPath"
    }

    $sql.Add("INSERT INTO ``$Db``.db_check_sheet (TestReportNo, SubFolder, FECOrderNo, Customer, Spec, PartNo, ShipDate, ShipQty, PrintDate, OperatorID, CheckSheet, Manual, ManualNo, Inuse) VALUES ('$($o.tr)', 1, '$($o.fec)', '$customer', '$spec', '$partNo', '$($o.d)', $($o.qty), NOW(), 'TEST', 1, 0, '', 1);")
    $rows = @()
    for ($n = 0; $n -lt $o.qty; $n++) {
        $rows += "('$($o.d)', '$($serials[$n])', 'TEST', NOW(), $keyId, '$customer', '$partNo', '$spec', '$($o.fec)', '$($o.tr)', '$note')"
    }
    $sql.Add("INSERT INTO ``$Db``.shipping (Ship_Date, Serial, SubLot, InsertTime, CusCond_ID, Customer, PartNo, Spec, FECOrderNo, TestReportNo, Note) VALUES " + ($rows -join ', ') + ";")
}

Set-Content -LiteralPath $SqlOut -Value $sql -Encoding UTF8
"done: $SqlOut"

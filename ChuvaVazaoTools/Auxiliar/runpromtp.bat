c:
cd\
cd\Users\marcos.albarracin\Downloads\GEFS

set /a "i = 6"

:while1

    if %i% leq 384 (
        if %i% leq 6 (
            perl g2ctl.gs geavg.t00z.pgrb2af0%i% > geavg.t00z.pgrb2af0%i%.ctl
            gribmap -i geavg.t00z.pgrb2af0%i%.ctl
            perl g2ctl.gs geavg.t06z.pgrb2af0%i% > geavg.t06z.pgrb2af0%i%.ctl
            gribmap -i geavg.t06z.pgrb2af0%i%.ctl
            perl g2ctl.gs geavg.t12z.pgrb2af0%i% > geavg.t12z.pgrb2af0%i%.ctl
            gribmap -i geavg.t12z.pgrb2af0%i%.ctl
            set /a "i = i + 6"
            goto :while1

)
            perl g2ctl.gs geavg.t00z.pgrb2af%i% > geavg.t00z.pgrb2af%i%.ctl
            gribmap -i geavg.t00z.pgrb2af%i%.ctl
            perl g2ctl.gs geavg.t06z.pgrb2af%i% > geavg.t06z.pgrb2af%i%.ctl
            gribmap -i geavg.t06z.pgrb2af%i%.ctl
            perl g2ctl.gs geavg.t12z.pgrb2af%i% > geavg.t12z.pgrb2af%i%.ctl
            gribmap -i geavg.t12z.pgrb2af%i%.ctl
            set /a "i = i + 6"
            goto :while1
)

pause
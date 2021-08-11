hora=%HORA%

if ( %HORA% = 00 )
  
**** previção 00 hrs ***************
  
  'reinit'
  
  day=%DIA%
  month=%MES%
  year=%ANO%
  
  
  i = 18
  
  while(i<=372)
*while(i<=36)
    'open geavg.t00z.pgrb2af'i'.ctl'
  i = i+6 
  endwhile
  
  'set display color white'
  'c'
  
  'sum = 0'
  j = 1
  k=1
  n=0
  summ = 0
  while (k<=60)
    
    while (j>=k & j<=k+3)
  
*while(j<=60)
*while(j<=4)
      'set dfile 'j''
      'set lon -75 -34.0'
      'set lat -35.0 5.0'
      'set t 1'
      'd apcpsfc'
      'define sum = sum + apcpsfc'
    j = j+1
    endwhile
  
    'set mpdset mresbr bacias'
  
    'set mpt 102 15 1 5'
*'set mpt 103 15 1 5'
    'set mpt 104 15 1 5'
    'set mpt 105 15 1 5'
*'set mpt 106 15 1 5'
    'set mpt 107 15 1 5'
    'set mpt 108 15 1 5'
    'set mpt 185 15 1 5'
    'set mpt 165 15 1 5'
    'set mpt 164 15 1 5'
    'set mpt 163 15 1 5'
    'set mpt 162 15 1 5'
    'set mpt 161 15 1 5'
    'set mpt 160 15 1 5'
    'set mpt 159 15 1 5'
    'set mpt 158 15 1 5'
    'set mpt 157 15 1 5'
    'set mpt 156 15 1 5'
    'set mpt 155 15 1 5'
    'set mpt 154 15 1 5'
    'set mpt 151 15 1 5'
    'set mpt 134 15 1 5'
    'set mpt 190 15 1 5'
    'set mpt 199 15 1 5'
  
    'set gxout shaded'
    'c'
    'set rgb 16 225 255 255'
    'set rgb 17 180 240 250'
    'set rgb 18 150 210 250'
    'set rgb 19 40 130 240'
    'set rgb 20 20 100 210'
    'set rgb 21 103 254 133'
    'set rgb 22 24 215 6'
    'set rgb 23 30 180 30'
    'set rgb 24 255 232 120'
    'set rgb 25 255 192 60'
    'set rgb 26 255 96 0'
    'set rgb 27 255 20 0'
    'set rgb 28 251 94 107'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'set grads off'
    
    'd sum'
    if(k<=37)
      'define summ = summ + sum'
    endif
    'cbarn 1 0'
    'set gxout contour'
    'set clopts 1'
    'set cthick 1'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'd sum'
  
    'draw string 4.5 10.9 GEFS' 
*  'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day'/0'month' ate 12Z 'day+1'/0'month''
*  'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
  
*******************************************************************************************************************
  
    if (month = 1 | month = 3 | month = 5 | month = 7 | month = 8)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-30'/0'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/0'month+1' ate 12Z 'day+n-30'/0'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 10)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-30'/'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/'month+1' ate 12Z 'day+n-30'/'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/'month'/'year''
      endif
    endif
  
    if (month = 12)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-30'/'month-11''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/'month+1' ate 12Z 'day+n-30'/'month-11''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/'month'/'year''
      endif
    endif
  
    if (month = 2)
      if (day + n < 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-27'/0'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-28'/0'month+1' ate 12Z 'day+n-27'/0'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 4 | month = 6)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-29'/0'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/0'month+1' ate 12Z 'day+n-29'/0'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 11)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/'month+1' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 9)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/'month+1' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 00Z dia 'day'/0'month'/'year''
      endif
    endif 
  
***********************************************************************************************************
  
    'printim prev00'n'.gif white'    

    'set fwrite prev00'n'.bin'
    'set gxout fwrite'
    'd sum'
    'disable fwrite'
    'set gxout shaded'
    'set gxout contour'
    
    'close 1'    
    
  k=k+4
  'sum=0'
  n=n+1
  
  endwhile

  'set mpdset mresbr bacias'
    
  'set mpt 102 15 1 5'
  *'set mpt 103 15 1 5'
  'set mpt 104 15 1 5'
  'set mpt 105 15 1 5'
  *'set mpt 106 15 1 5'
  'set mpt 107 15 1 5'
  'set mpt 108 15 1 5'
  'set mpt 185 15 1 5'
  'set mpt 165 15 1 5'
  'set mpt 164 15 1 5'
  'set mpt 163 15 1 5'
  'set mpt 162 15 1 5'
  'set mpt 161 15 1 5'
  'set mpt 160 15 1 5'
  'set mpt 159 15 1 5'
  'set mpt 158 15 1 5'
  'set mpt 157 15 1 5'
  'set mpt 156 15 1 5'
  'set mpt 155 15 1 5'
  'set mpt 154 15 1 5'
  'set mpt 151 15 1 5'
  'set mpt 134 15 1 5'
  'set mpt 190 15 1 5'
  'set mpt 199 15 1 5'
    
  'set gxout shaded'
  'c'
  'set rgb 16 225 255 255'
  'set rgb 17 180 240 250'
  'set rgb 18 150 210 250'
  'set rgb 19 40 130 240'
  'set rgb 20 20 100 210'
  'set rgb 21 103 254 133'
  'set rgb 22 24 215 6'
  'set rgb 23 30 180 30'
  'set rgb 24 255 232 120'
  'set rgb 25 255 192 60'
  'set rgb 26 255 96 0'
  'set rgb 27 255 20 0'
  'set rgb 28 251 94 107'
  'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
  'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
  'set grads off'
    
  'd summ'
  'cbarn 1 0'
  'set gxout contour'
  'set clopts 1'
  'set cthick 1'
  'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
  'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
  'd summ'
    
  'draw string 4.5 10.9 GEFS' 
  'draw string 4.5 10.5 Precipitacao (mm) acumulada dos ultimos 10 dias'
  'draw string 4.5 10.1 Previsao das 00Z dia 'day'/'month'/'year''
      
  'printim prev_acumulado.gif white'
  'close 1'

endif

if ( %HORA% = 06 )
  
**** previção 06 hrs ***************
  
  'reinit'
  
  day=%DIA%
  month=%MES%
  year=%ANO%
  
  i = 12
  
  while(i<=366)
*while(i<=30)
    'open geavg.t06z.pgrb2af'i'.ctl'
  i = i+6 
  endwhile
  
  'set display color white'
  'c'
  
  'sum = 0'
  j = 1
  k=1
  n=0
  
  while (k<=60)
  
    while (j>=k & j<=k+3)
  
*while(j<=60)
*while(j<=4)
      'set dfile 'j''
      'set lon -75 -34.0'
      'set lat -35.0 5.0'
      'set t 1'
      'd apcpsfc'
      'define sum = sum + apcpsfc'
    j = j+1
    endwhile
  
    'set mpdset mresbr bacias'
  
    'set mpt 102 15 1 5'
*'set mpt 103 15 1 5'
    'set mpt 104 15 1 5'
    'set mpt 105 15 1 5'
*'set mpt 106 15 1 5'
    'set mpt 107 15 1 5'
    'set mpt 108 15 1 5'
    'set mpt 185 15 1 5'
    'set mpt 165 15 1 5'
    'set mpt 164 15 1 5'
    'set mpt 163 15 1 5'
    'set mpt 162 15 1 5'
    'set mpt 161 15 1 5'
    'set mpt 160 15 1 5'
    'set mpt 159 15 1 5'
    'set mpt 158 15 1 5'
    'set mpt 157 15 1 5'
    'set mpt 156 15 1 5'
    'set mpt 155 15 1 5'
    'set mpt 154 15 1 5'
    'set mpt 151 15 1 5'
    'set mpt 134 15 1 5'
    'set mpt 190 15 1 5'
    'set mpt 199 15 1 5'
  
    'set gxout shaded'
    'c'
    'set rgb 16 225 255 255'
    'set rgb 17 180 240 250'
    'set rgb 18 150 210 250'
    'set rgb 19 40 130 240'
    'set rgb 20 20 100 210'
    'set rgb 21 103 254 133'
    'set rgb 22 24 215 6'
    'set rgb 23 30 180 30'
    'set rgb 24 255 232 120'
    'set rgb 25 255 192 60'
    'set rgb 26 255 96 0'
    'set rgb 27 255 20 0'
    'set rgb 28 251 94 107'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'set grads off'
  
    'd sum'
    'cbarn 1 0'
    'set gxout contour'
    'set clopts 1'
    'set cthick 1'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'd sum'
  
    'draw string 4.5 10.9 GEFS'
*  'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day'/0'month' ate 12Z 'day+1'/0'month''
*  'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
  
*********************************************************************************************************************
  
    if (month = 1 | month = 3 | month = 5 | month = 7 | month = 8)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-30'/0'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/0'month+1' ate 12Z 'day+n-30'/0'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 10)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-30'/'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/'month+1' ate 12Z 'day+n-30'/'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/'month'/'year''
      endif
    endif
  
    if (month = 12)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-30'/'month-11''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/'month+1' ate 12Z 'day+n-30'/'month-11''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/'month'/'year''
      endif
    endif
  
    if (month = 2)
      if (day + n < 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-27'/0'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-28'/0'month+1' ate 12Z 'day+n-27'/0'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 4 | month = 6)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-29'/0'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/0'month+1' ate 12Z 'day+n-29'/0'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 11)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/'month+1' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 9)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/'month+1' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 06Z dia 'day'/0'month'/'year''
      endif
    endif 
  
***********************************************************************************************************
  
    'printim prev06'n'.gif white'
    
    'set fwrite prev06'n'.bin'
    'set gxout fwrite'
    'd sum'
    'disable fwrite'
    'set gxout shaded'
    'set gxout contour'
    
    'close 1'
  
  k=k+4
  'sum=0'
  n=n+1
  
  endwhile
  
endif

if ( %HORA% = 12 )

**** previção 12 hrs ***************
  
  'reinit'
  
  day=%DIA%
  month=%MES%
  year=%ANO%
  
  i = 06
  
  while(i<=360)
*while(i<=24)
    'open geavg.t12z.pgrb2af'i'.ctl'
  i = i+6 
  endwhile
  
  'set display color white'
  'c'
  
  'sum = 0'
  j = 1
  k=1
  n=0
  summ = 0
  while (k<=60)
  
    while (j>=k & j<=k+3)
  
*while(j<=60)
*while(j<=4)
      'set dfile 'j''
      'set lon -75 -34.0'
      'set lat -35.0 5.0'
      'set t 1'
      'd apcpsfc'
      'define sum = sum + apcpsfc'
    j = j+1
    endwhile
  
    'set mpdset mresbr bacias'
  
    'set mpt 102 15 1 5'
*'set mpt 103 15 1 5'
    'set mpt 104 15 1 5'
    'set mpt 105 15 1 5'
*'set mpt 106 15 1 5'
    'set mpt 107 15 1 5'
    'set mpt 108 15 1 5'
    'set mpt 185 15 1 5'
    'set mpt 165 15 1 5'
    'set mpt 164 15 1 5'
    'set mpt 163 15 1 5'
    'set mpt 162 15 1 5'
    'set mpt 161 15 1 5'
    'set mpt 160 15 1 5'
    'set mpt 159 15 1 5'
    'set mpt 158 15 1 5'
    'set mpt 157 15 1 5'
    'set mpt 156 15 1 5'
    'set mpt 155 15 1 5'
    'set mpt 154 15 1 5'
    'set mpt 151 15 1 5'
    'set mpt 134 15 1 5'
    'set mpt 190 15 1 5'
    'set mpt 199 15 1 5'
  
    'set gxout shaded'
    'c'
    'set rgb 16 225 255 255'
    'set rgb 17 180 240 250'
    'set rgb 18 150 210 250'
    'set rgb 19 40 130 240'
    'set rgb 20 20 100 210'
    'set rgb 21 103 254 133'
    'set rgb 22 24 215 6'
    'set rgb 23 30 180 30'
    'set rgb 24 255 232 120'
    'set rgb 25 255 192 60'
    'set rgb 26 255 96 0'
    'set rgb 27 255 20 0'
    'set rgb 28 251 94 107'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'set grads off'
  
    'd sum'
    if(k<=37)
      'define summ = summ + sum'
    endif
    'cbarn 1 0'
    'set gxout contour'
    'set clopts 1'
    'set cthick 1'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'd sum'
  
    'draw string 4.5 10.9 GEFS' 
*  'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day'/0'month' ate 12Z 'day+1'/0'month''
*  'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
  
*********************************************************************************************************************
  
    if (month = 1 | month = 3 | month = 5 | month = 7 | month = 8)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-30'/0'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/0'month+1' ate 12Z 'day+n-30'/0'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 10)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-30'/'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/'month+1' ate 12Z 'day+n-30'/'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/'month'/'year''
      endif
    endif
  
    if (month = 12)
      if (day + n < 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/'month'/'year''
      endif
      if (day + n = 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-30'/'month-11''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 31)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-31'/'month+1' ate 12Z 'day+n-30'/'month-11''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/'month'/'year''
      endif
    endif
  
    if (month = 2)
      if (day + n < 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-27'/0'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 28)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-28'/0'month+1' ate 12Z 'day+n-27'/0'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 4 | month = 6)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-29'/0'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/0'month+1' ate 12Z 'day+n-29'/0'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 11)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n+1'/'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/'month' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/'month+1' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
    endif
  
    if (month = 9)
      if (day + n < 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n+1'/0'month''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n = 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n'/0'month' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
      if (day + n > 30)
        'draw string 4.5 10.5 Precipitacao (mm) acumulada entre 12Z 'day+n-30'/'month+1' ate 12Z 'day+n-29'/'month+1''
        'draw string 4.5 10.1 Previsao das 12Z dia 'day'/0'month'/'year''
      endif
    endif 
  
***********************************************************************************************************
  
    'printim prev12'n'.gif white'
    
    'set fwrite prev12'n'.bin'
    'set gxout fwrite'
    'd sum'
    'disable fwrite'
    'set gxout shaded'
    'set gxout contour'
    
    'close 1'
  
  k=k+4
  'sum=0'
  n=n+1
  
  endwhile
  
  'set mpdset mresbr bacias'
    
  'set mpt 102 15 1 5'
  *'set mpt 103 15 1 5'
  'set mpt 104 15 1 5'
  'set mpt 105 15 1 5'
  *'set mpt 106 15 1 5'
  'set mpt 107 15 1 5'
  'set mpt 108 15 1 5'
  'set mpt 185 15 1 5'
  'set mpt 165 15 1 5'
  'set mpt 164 15 1 5'
  'set mpt 163 15 1 5'
  'set mpt 162 15 1 5'
  'set mpt 161 15 1 5'
  'set mpt 160 15 1 5'
  'set mpt 159 15 1 5'
  'set mpt 158 15 1 5'
  'set mpt 157 15 1 5'
  'set mpt 156 15 1 5'
  'set mpt 155 15 1 5'
  'set mpt 154 15 1 5'
  'set mpt 151 15 1 5'
  'set mpt 134 15 1 5'
  'set mpt 190 15 1 5'
  'set mpt 199 15 1 5'
    
  'set gxout shaded'
  'c'
  'set rgb 16 225 255 255'
  'set rgb 17 180 240 250'
  'set rgb 18 150 210 250'
  'set rgb 19 40 130 240'
  'set rgb 20 20 100 210'
  'set rgb 21 103 254 133'
  'set rgb 22 24 215 6'
  'set rgb 23 30 180 30'
  'set rgb 24 255 232 120'
  'set rgb 25 255 192 60'
  'set rgb 26 255 96 0'
  'set rgb 27 255 20 0'
  'set rgb 28 251 94 107'
  'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
  'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
  'set grads off'
    
  'd summ'
  'cbarn 1 0'
  'set gxout contour'
  'set clopts 1'
  'set cthick 1'
  'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
  'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
  'd summ'
    
  'draw string 4.5 10.9 GEFS' 
  'draw string 4.5 10.5 Precipitacao (mm) acumulada dos ultimos 10 dias'
  'draw string 4.5 10.1 Previsao das 12Z dia 'day'/'month'/'year''
      
  'printim prev_acumulado.gif white'
  'close 1'

endif

'quit'

*******
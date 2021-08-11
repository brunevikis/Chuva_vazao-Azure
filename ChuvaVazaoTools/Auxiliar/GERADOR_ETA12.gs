hora=%HORA%

if ( %HORA% = 00 )
** Reinicia o sistema **
  
  'reinit'   
  
**
  
** Definir o dia - mês - ano **
  
  day=%DIA%
  month=%MES%
  year=%ANO%
  
  
**
  
** Parametrização do dia - mês - ano **
  
  i=day
  j=month
  k=year
  
**
  
** Horario inicial (exemplo:pp20180723_00m) e parâmetro para arquivo gif (numeração de figuras) ** 
  
  m=36
  n=0
  
**
  
** Inicio do Loop Principal**
  
  while (m<=252)
  
    if(day >=10)
      
      if (month<10)
  
** Condicional para abertura de arquivos **
  
        if (m<=84)   
          'open pp'k'0'j''i'_00'm'.ctl'              
        else
  
          'open pp'k'0'j''i'_0'm'.ctl' 
          
        endif
**      
      endif
  
      if (month >= 10)
        
        if (m<=84) 

          'open pp'k''j''i'_00'm'.ctl'    
        
        else
  
          'open pp'k''j''i'_0'm'.ctl'  
  
        endif
      
      endif
  
    else
  
      if (month<10)
  
** Condicional para abertura de arquivos **
  
        if (m<=84)   
 
          'open pp'k'0'j'0'i'_00'm'.ctl'  
            
        else
  
          'open pp'k'0'j'0'i'_0'm'.ctl'
            
        endif
**
      
      endif
  
      if (month >= 10)
        
        if (m<=84) 
  
          'open pp'k''j'0'i'_00'm'.ctl'    
           
        else
  
          'open pp'k''j'0'i'_0'm'.ctl'  
     
        endif
      
      endif
  
    endif
  
** Bloco de especificações (tipo de mapas, carregamento dos contornos, grade, grafico da precipitação e clasificação de cores) ** 
  
    'set display color white'
    'clear'
    'query file 1'
    'set lon -75 -34.0'
    'set lat -35.0 5.0'
    'set t 1'
  
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
  
    'display prec'
    'cbarn 1 0'
    'set gxout contour'
    'set clopts 1'
    'set cthick 1'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'display prec'
  
**
  
** Bloco de rotulação de figuras **
  
    'draw string 4.5 10.9 Modelo Regional / Brasil' 
      
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
  
** Criação de figuras e fechamento do historial **
  
    'printim %CAMINHO%prev'n+1'.gif white'
    'close 1'
  
**
  
    m = m + 24
    n = n + 1
  
  endwhile
  
** Final do Loop Principal **

endif


if ( %HORA% = 12 )
** Reinicia o sistema **
  
  'reinit'   
  
**
  
** Definir o dia - mês - ano **
  
  day=%DIA%
  month=%MES%
  year=%ANO%
  
  
**
  
** Parametrização do dia - mês - ano **
  
  i=day
  j=month
  k=year
  
**
  
** Horario inicial (exemplo:pp20180723_00m) e parâmetro para arquivo gif (numeração de figuras) ** 
  
  m=24
  n=0
  
**
  
** Inicio do Loop Principal**
  
  while (m<=240)
  
    if(day >=10)
      
      if (month<10)
  
** Condicional para abertura de arquivos **
  
        if (m<=96) 
  
** arquivos para 12 horas **
  
          'open pp'k'0'j''i'_00'm'.ctl'  
    
* arquivos para 00 horas **
    
*'open pp20180723_0036.ctl'    
            
        else
  
** arquivos para 12 horas **
  
          'open pp'k'0'j''i'_0'm'.ctl'
        
** arquivos para 00 horas **
  
*'open pp20180723_0036.ctl'  
            
        endif
**
      
      endif
  
      if (month >= 10)
        
        if (m<=96) 
  
** arquivos para 12 horas ***
  
          'open pp'k''j''i'_00'm'.ctl'    
   
** arquivos para 00 horas **  
  
*'open pp20180723_0036.ctl'     
           
        else
  
** arquivos para 12 horas ** 
  
          'open pp'k''j''i'_0'm'.ctl'  
  
** arquivos para 00 horas **
       
*'open pp20180723_0036.ctl'          
     
        endif
      
      endif
  
    else
  
      if (month<10)
  
** Condicional para abertura de arquivos **
  
        if (m<=96) 
  
** arquivos para 12 horas **
  
          'open pp'k'0'j'0'i'_00'm'.ctl'  
    
* arquivos para 00 horas **
    
*'open pp20180723_0036.ctl'    
            
        else
  
** arquivos para 12 horas **
  
          'open pp'k'0'j'0'i'_0'm'.ctl'
        
** arquivos para 00 horas **
  
*'open pp20180723_0036.ctl'  
            
        endif
**
      
      endif
  
      if (month >= 10)
        
        if (m<=96) 
  
** arquivos para 12 horas ***
  
          'open pp'k''j'0'i'_00'm'.ctl'    
   
** arquivos para 00 horas **  
  
*'open pp20180723_0036.ctl'     
           
        else
  
** arquivos para 12 horas ** 
  
          'open pp'k''j'0'i'_0'm'.ctl'  
  
** arquivos para 00 horas **
       
*'open pp20180723_0036.ctl'          
     
        endif
      
      endif
  
    endif
  
** Bloco de especificações (tipo de mapas, carregamento dos contornos, grade, grafico da precipitação e clasificação de cores) ** 
  
    'set display color white'
    'clear'
    'query file 1'
    'set lon -75 -34.0'
    'set lat -35.0 5.0'
    'set t 1'
  
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
  
    'display prec'
    'cbarn 1 0'
    'set gxout contour'
    'set clopts 1'
    'set cthick 1'
    'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200'
    'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 15'
    'display prec'
  
**
  
** Bloco de rotulação de figuras **
  
    'draw string 4.5 10.9 Modelo Regional / Brasil' 
      
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
  
** Criação de figuras e fechamento do historial **
  
    'printim %CAMINHO%prev'n+1'.gif white'
    'close 1'
  
**
  
    m = m + 24
    n = n + 1
  
  endwhile
  
** Final do Loop Principal **

endif

'quit'

*******
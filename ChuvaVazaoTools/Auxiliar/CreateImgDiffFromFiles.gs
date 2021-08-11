*VARIABLES
*% FILECOUNT% 
*% OPENFILES%
*    'open pp'year'0'month'0'day'_0036.ctl'
*    'open pp'year'0'month'0'day'_0060.ctl'
*    'open pp'year'0'month'0'day'_0084.ctl'
*    'open pp'year'0'month'0'day'_0108.ctl'
*    'open pp'year'0'month'0'day'_0132.ctl'
*    'open pp'year'0'month'0'day'_0156.ctl'
*    'open pp'year'0'month'0'day'_0180.ctl'
*    'open pp'year'0'month'0'day'_0204.ctl'
*    'open pp'year'0'month'0'day'_0228.ctl'
*    'open pp'year'0'month'0'day'_0252.ctl'
*% VARIABLE%
*    prec or apcpsfc
*% HEADER_MODELO%
*% HEADER_TITULO%
*% HEADER_DATA%
*   'draw string 4.5 10.9 % HEADER_MODELO%' 
*   'draw string 4.5 10.5 % HEADER_TITULO%'
*   'draw string 4.5 10.1 % HEADER_DATA%'
*% GIFFILE%

'reinit'   

%OPENFILESA%   
    
'sumA = 0'
i = 1
while(i <= %FILECOUNTA%)
    
  'set display color white'
  'clear'
  'set dfile 'i''
  'set lon -75 -34.0'
  'set lat -35.0 5.0'
  'set t 1'  
  'display %VARIABLE%'
  'define p'i' = %VARIABLE%'  
  i = i + 1
  
endwhile

somaA="define sumA = re(p1, 0.4)"
i = 2
while(i <= %FILECOUNTA%)
  somaA = somaA% " + re(p"i", 0.4)"
  i = i + 1
endwhile

''somaA''

%OPENFILESB%   
    
'sumB = 0'
i = %FILECOUNTA% + 1
while(i <= %FILECOUNTA% + %FILECOUNTB%)
    
  'set display color white'
  'clear'
  'set dfile 'i''
  'set lon -75 -34.0'
  'set lat -35.0 5.0'
  'set t 1'  
  'display %VARIABLE%'
  'define p'i' = %VARIABLE%'  
  i = i + 1
  
endwhile

i = %FILECOUNTA% + 1
somaB="define sumB = re(p"i", 0.4)"
i = i + 1
while(i <= %FILECOUNTA% + %FILECOUNTB%)
  somaB = somaB% " + re(p"i", 0.4)"
  i = i + 1
endwhile

''somaB''



'define sum = sumA - sumB'


'set mpdset mresbr bacias'
  
'set mpt 102 15 1 5'
'set mpt 104 15 1 5'
'set mpt 105 15 1 5'
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
'set rgb 29 184 78 176'
'set clevs -150 -75 -50 -30 -20 -10 -5 0 5 10 20 30 50 75 150'
'set ccols 15 29 28 27 26 25 24 0 16 17 18 19 20 21 23'
'set grads off'
  
'd sum'
'cbarn 1 0'
'set gxout contour'
'set clopts 1'
'set cthick 1'
'set clevs -150 -75 -50 -30 -20 -10 -5 0 5 10 20 30 50 75 150'
'set ccols 15 29 28 27 26 25 24 0 16 17 18 19 20 21 23'
'd sum'
  
'draw string 4.5 10.9 %HEADER_MODELO%' 
'draw string 4.5 10.5 %HEADER_TITULO%'
'draw string 4.5 10.1 %HEADER_DATA%'
    
'printim %GIFFILE% white'

i = %FILECOUNTA% + %FILECOUNTB%
while(i > 0)  
  'close 'i''  
  i = i - 1
endwhile

'quit'

*******
function main(args)
'reinit' 
a = subwrd(args,1)
i = subwrd(args,2)
n = subwrd(args,3)
s1 = subwrd(args,4)
s2.1 = subwrd(args,5)
s2.2 = subwrd(args,6)
s3 = subwrd(args,7)
*'gfs.t00z.pgrb2.0p25.f'

k = i - 18
 file = a%k
 say file
'open '%file%'.ctl'
k = i - 12
 file = a%k
 say file
'open '%file%'.ctl'
k = i - 6
 file = a%k
 say file
'open '%file%'.ctl'
 file = a%i
 say file
'open '%file%'.ctl'
    
'sum = 0'

  'set display color white'
  'clear'
  'set dfile '1''
  'set lon -75 -34.0'
  'set lat -35.0 5.0'
  'set t 1'  
  'display APCPsfc'
  'define p'1' = APCPsfc'
    
  'set display color white'
  'clear'
  'set dfile '2''
  'set lon -75 -34.0'
  'set lat -35.0 5.0'
  'set t 1'  
  'display APCPsfc'
  'define p'2' = APCPsfc'
  
    'set display color white'
  'clear'
  'set dfile '3''
  'set lon -75 -34.0'
  'set lat -35.0 5.0'
  'set t 1'  
  'display APCPsfc'
  'define p'3' = APCPsfc'
  
    'set display color white'
  'clear'
  'set dfile '4''
  'set lon -75 -34.0'
  'set lat -35.0 5.0'
  'set t 1'  
  'display APCPsfc'
  'define p'4' = APCPsfc'

'define sum = p'4' + p'3' + p'2' + p'1
*'define sum = p'4'

'set mpdset /cygdrive/c/NOAA/scripts/mresbr /cygdrive/c/NOAA/scripts/bacias'
  
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
'set rgb 29 184 78 176'
'set rgb 30 121 36 108'
'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200 300 400'
'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 15'
'set grads off'
  
'd sum'
'cbarn 1 0'
'set gxout contour'
'set clopts 1'
'set cthick 1'
'set clevs 0 1 5 10 15 20 25 30 40 50 75 100 150 200 300 400'
'set ccols 0 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 15'
'd sum'
r = n*24+12-i
'draw string 4.5 10.9 '%s1 
'draw string 4.5 10.5 Preciptacao(mm) acumulada entre 12Z '%s2.1%' ate 12Z '%s2.2
'draw string 4.5 10.1 Previsao das '%substr(0%r,strlen(r),2)%'Z do dia '%s3

'printim prev'%n%'.gif white'
  
  'set fwrite '%file%'.bin'
  'set gxout fwrite'
  'd sum'
  'disable fwrite'
  'set gxout shaded'
  'set gxout contour'
  
  
outfile=file%'.dat'
'set gxout print'
fmt = '%8.2f'
'set prnopts 'fmt' 81 1'
'd lon'
lon_data = result
'd lat'
lat_data = result
'd sum'
PREC = result
i=1
while (1)
  lons  = sublin(lon_data,i)
  lats  = sublin(lat_data,i)
  precs = sublin(PREC,i)
  if (lons='' | lats='' | precs=''); break; endif
  if (i>1)
    j=1
    while (j<=81)
      str = subwrd(lons,j); lon = math_format('%8.2f',str)
      str = subwrd(lats,j); lat = math_format('%8.2f',str)
      str = subwrd(precs,j); v1 = math_format('%8.2f',str)
      record = lon' 'lat' 'v1
      rc = write(outfile,record,append)
      j=j+1
    endwhile
  endif
  i=i+1
endwhile


'close 4'
'close 3'
'close 2'
'close 1'

'quit'
*******

import png

r=png.Reader('input.png').read()

print(r)
print(r[2])


newRows = r[2]


myPng = []

# read in the png to this data structure
for row in r[2]:
    myRow = []
    for byte in row:
        myRow.append(byte)
    myPng.append(myRow)

# translate white level to alpha

for row in range(128):
    for i in range(128):
        thisAlpha = 0
        for j in range(4):
            if j is 0:
                thisAlpha = myPng[row][4*i + j]
                myPng[row][4*i + j] = 255
            if j is 1:
                myPng[row][4*i + j] = 255
            if j is 2:
                myPng[row][4*i + j] = 255
            if j is 3:
                myPng[row][4*i + j] = thisAlpha


f = open('output.png', 'wb')
w = png.Writer(width=128, height=128, greyscale=False, alpha=True)
w.write(f, myPng)
f.close()

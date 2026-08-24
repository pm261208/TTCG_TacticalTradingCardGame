import cv2
import numpy as np

images = ["RedToGreenColorGradiant.png"]

# Carregar imagem
for i in images:
    img = cv2.imread(i)
    img = cv2.cvtColor(img, cv2.COLOR_BGR2BGRA)

    # Definir cor alvo (exemplo: verde)
    lower = np.array([224, 224, 224, 255])
    upper = np.array([255, 255, 255, 255])

    # Criar máscara
    mask = cv2.inRange(img, lower, upper)

    # Tornar transparente
    img[mask != 0] = [0, 0, 0, 0]

    cv2.imwrite(i[:-4]+"T.png", img)
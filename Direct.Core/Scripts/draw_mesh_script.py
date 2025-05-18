import os
import os.path
import matplotlib.pyplot as plt
import numpy as np
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

units = []

with open("output.txt", "r") as f:
    nums = f.readline()[:-1]
    for i in range(int(nums)):
        units.append([float(j) for j in f.readline()[:-2].replace(',', '.').split(' ')])

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

if __name__ == '__main__':
    for i in range(int(nums)):
        points = np.array([[units[i][0], units[i][2], units[i][4]],
                           [units[i][1], units[i][2], units[i][4]],
                           [units[i][1], units[i][3], units[i][4]],
                           [units[i][0], units[i][3], units[i][4]],
                           [units[i][0], units[i][2], units[i][5]],
                           [units[i][1], units[i][2], units[i][5]],
                           [units[i][1], units[i][3], units[i][5]],
                           [units[i][0], units[i][3], units[i][5]]])

        Z = np.zeros((8, 3))
        for i in range(8):
            Z[i, :] = np.dot(points[i, :], 1)

        # plot vertices
        ax.scatter(Z[:, 0], Z[:, 1], Z[:, 2], c='k', s=1)

        # list of sides' polygons of figure
        verts = [[Z[0], Z[1], Z[2], Z[3]],
                 [Z[4], Z[5], Z[6], Z[7]],
                 [Z[0], Z[1], Z[5], Z[4]],
                 [Z[2], Z[3], Z[7], Z[6]],
                 [Z[1], Z[2], Z[6], Z[5]],
                 [Z[4], Z[7], Z[3], Z[0]]]

        # plot sides
        ax.add_collection3d(Poly3DCollection(verts, linewidths=.3, edgecolors='b', alpha=.1))

ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')

directory = "OutputPlots"
os.mkdir(directory)

imgs = [(90, -90, 0), (0, -90, 0), (0, 0, 0)]

for i, img in enumerate(imgs):
    file_name = "plot" + str(i) + ".png"
    path = os.path.join(directory, file_name)
    ax.view_init(elev=img[0], azim=img[1], roll=img[2])
    plt.savefig(path)
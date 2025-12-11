import cv2
import matplotlib.pyplot as plt
import mediapipe as mp
import time
import numpy as np
from playsound import playsound
import threading
from matplotlib.animation import FuncAnimation
import socket

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
mp_drawing_styles = mp.solutions.drawing_styles
#选择需要的解决方案，手部检测就mp_hands=mp.solutions.hands,其他类似
mp_holistic = mp.solutions.holistic

# 读取保存姿势数据的文件
file_path = 'body_positions.txt'
# 存储读取的姿势数据
positions = []
# 打开文件并读取数据
with open(file_path, 'r') as f:
    lines = f.readlines()
    for line in lines:
        # 去除末尾的换行符并按逗号分隔
        data = line.strip().split(', ')
        # 将数据转换为浮点数列表
        position = [float(coord) for coord in data]
        # 将每组(x, y)坐标对添加到positions列表中
        positions.append(position)

# 计算矩形的四个角点
for idx, positions in enumerate(positions):
    # print(f"Frame {idx}:")#Frame表示每帧记录的数据
    for i in range(0, len(positions), 3):
        x, y, z = positions[i], positions[i + 1], positions[i + 2]
        # print(f"Body landmark {i // 3 + 1} - x: {x}, y: {y}, z: {z}")
        # 计算线段的y值
        LineY = (positions[1] + positions[3] + positions[5] + positions[7] + positions[9] +
                 positions[11]) / 6
        # print("LinY" + str(LineY))
        # 计算线段的中点
        LinePoint1 = positions[4]
        LinePoint2 = positions[10]
        midpoint_x = (LinePoint1 + LinePoint2) / 2
        # 计算线段的长度
        segment_length = abs(LinePoint1 - LinePoint2)
        # 假设正方形边长为线段长度的一半
        square_length = segment_length / 2
        # 计算正方形的四个顶点
        square_points_1x1 = np.array([
            (midpoint_x - square_length / 2, LineY - square_length / 2),
            (midpoint_x + square_length / 2, LineY - square_length / 2),
            (midpoint_x + square_length / 2, LineY + square_length / 2),
            (midpoint_x - square_length / 2, LineY + square_length / 2)
            ], np.float32)


#生成三维坐标系点
colorclass = plt.cm.ScalarMappable(cmap='jet')
colors = colorclass.to_rgba(np.linspace(0, 1, int(33)))
colormap = (colors[:, 0:3])

def draw3d(plt, ax, world_landmarks, connnection=mp_pose.POSE_CONNECTIONS):
    ax.clear()
    # 坐标原点
    ax.set_xlim3d(-1, 1)
    ax.set_ylim3d(-1, 1)
    ax.set_zlim3d(-1, 1)

    landmarks = []
    for index, landmark in enumerate(world_landmarks.landmark):
        landmarks.append([landmark.x, landmark.z, landmark.y*(-1)])
    landmarks = np.array(landmarks)

    ax.scatter(landmarks[:, 0], landmarks[:, 1], landmarks[:, 2], c=np.array(colormap), s=50)
    for _c in connnection:
        ax.plot([landmarks[_c[0], 0], landmarks[_c[1], 0]],
                [landmarks[_c[0], 1], landmarks[_c[1], 1]],
                [landmarks[_c[0], 2], landmarks[_c[1], 2]], 'k')

    plt.pause(0.001)

# 获取摄像头实时视频
cap = cv2.VideoCapture(1)
# 检查摄像头是否成功打开
if not cap.isOpened():
    print("Error: Could not open video.")
    exit()

# 获取摄像头视频的宽度和高度
image_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
image_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
print("宽" + str(image_width))
print("高" + str(image_height))

# 将1x1空间内的矩形坐标映射到摄像头视频上
def map_Rec_to_image(Rec_1x1, image_width, image_height):
    # 创建一个空白图像
    image = np.zeros((image_height, image_width, 3), dtype=np.uint8)
    # 定义图像上的四个顶点（这里以图像中心为原点）
    image_points = np.array([
        (image_width * Rec_1x1[0, 0], image_height * (1 - Rec_1x1[0, 1])),
        (image_width * Rec_1x1[1, 0], image_height * (1 - Rec_1x1[1, 1])),
        (image_width * Rec_1x1[2, 0], image_height * (1 - Rec_1x1[2, 1])),
        (image_width * Rec_1x1[3, 0], image_height * (1 - Rec_1x1[3, 1]))
    ], np.int32)
    # print("矩形角点"+image_points)
    image_points = image_points.reshape((-1, 1, 2))
    # 绘制正方形
    cv2.polylines(image, [image_points], isClosed=True, color=(255, 0, 0), thickness=2)
    return image


# 将1x1空间内的点坐标映射到摄像头视频上
def map_points_to_image(points_1x1, image_width, image_height):
    # 创建一个空白图像
    image = np.zeros((image_height, image_width, 3), dtype=np.uint8)
    # 将1x1空间内的点坐标映射到图像上
    image_point = (
        int(image_width * points_1x1[0]),    # x 坐标
        int(image_height * points_1x1[1])  # y 坐标，1 - point_1x1[1] 是因为图像的坐标系与1x1空间的坐标系在 y 轴上是反向的
    )
    # print("点"+image_point)
    # 绘制圆点
    cv2.circle(image, image_point, 5, (0, 0, 255), -1)  # 5 是圆的半径，(0, 0, 255) 是红色
    return image

# 检测人体姿态
with mp_holistic.Holistic(
        model_complexity=0,  # 分析质量，取0、1、2，越大质量越好，帧数会降低
        min_detection_confidence=0.5,  # 检测置信度阈值
        min_tracking_confidence=0.5) as holistic:  # 追踪置信度阈值
    fig = plt.figure()
    ax = fig.add_subplot(111, projection="3d")

    while True:
        # 从摄像头读取一帧图像
        ret, frame = cap.read()
        if not ret:
            print("Error: Failed to capture frame.")
            continue

        start = time.time()
        frame.flags.writeable = False
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)  # BGR图转RGB
        results = holistic.process(frame)  # 处理三通道彩色图,返回了结点坐标,可以按需调用坐标
        # 绘制关键点并获取特定关键点的坐标
        if results.pose_landmarks:
            # 获取特定关键点的坐标
            landmark_11 = results.pose_landmarks.landmark[11]  # 身体点11-肩膀
            landmark_12 = results.pose_landmarks.landmark[12]  # 身体点12-肩膀
            landmark_18 = results.pose_landmarks.landmark[18]  # 身体点19-手
            landmark_20 = results.pose_landmarks.landmark[20]  # 身体点19-手
            landmark_22 = results.pose_landmarks.landmark[22]  # 身体点19-手
            # print("landmark11" + str(landmark_11))
        # 在图像上绘制特定关键点
            # 绘制身体点12、14、16
            # for landmark in [landmark_12,  landmark_11, landmark_18, landmark_20, landmark_22]:
            #     cx, cy = int(landmark.x * frame.shape[1]), int(landmark.y * frame.shape[0])
            #     cv2.circle(frame, (cx, cy), 5, (0, 0, 255), -1)  # 画圆点

        # 追踪圆点的坐标
        # points_1x1 = (0.5, 0.2)
        # points_1x1 =( (landmark_11.x+landmark_12.x) / 2, (landmark_11.y+landmark_12.y) / 2)#肩膀中心
        # print("points_1x1" + str(points_1x1))
        #追踪手的位置
        points_hand = (landmark_18.x , landmark_18.y)  # 手


        # 将1x1空间内的正方形映射到摄像头图像上
        mapped_image = map_Rec_to_image(square_points_1x1, image_width, image_height)
        # 将1x1空间内的圆点映射到摄像头图像上
        # mapped_image2 = map_points_to_image(points_1x1, image_width, image_height)
        mapped_image3 = map_points_to_image(points_hand, image_width, image_height)
        # 在摄像头图像上叠加绘制的正方形、圆点
        result_frame1 = cv2.addWeighted(frame, 1, mapped_image,1, 0)
        # result_frame = cv2.addWeighted(result_frame1, 1, mapped_image2 ,1, 0)
        result_frame = cv2.addWeighted(result_frame1, 1, mapped_image3 ,1, 0)

        #显示处理后的视频帧
        cv2.imshow('Mapped Square on Video', result_frame)

        # 按下'q'键退出循环
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break










# # 更新函数，用于每一帧动画更新矩形框的位置
# def update(frame):
#     x = positions[12]  # 第14个点的x坐标
#     y = positions[13]  # 第14个点的y坐标
#     result_frame.set_xy((x - 0.05, y - 0.05))  # 设置矩形框左下角的坐标，使其以点14为中心
#     return result_frame,
#
# # 创建动画
# ani = FuncAnimation(result_frame, update, frames=len(positions), interval=50, blit=True)

# 释放摄像头和关闭所有窗口
cap.release()
cv2.destroyAllWindows()




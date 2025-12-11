import cv2
import matplotlib.pyplot as plt
import mediapipe as mp
import time
import numpy as np
import pygame
from playsound import playsound
import threading

mp_drawing = mp.solutions.drawing_utils
mp_pose = mp.solutions.pose
mp_drawing_styles = mp.solutions.drawing_styles
#选择需要的解决方案，手部检测就mp_hands=mp.solutions.hands,其他类似
mp_holistic = mp.solutions.holistic

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

# 初始化变量
start_time = time.time()  # 记录开始时间
in_horizontal_position = False  # 标记是否处于水平位置
# 初始化pygame
pygame.init()
# 加载音频文件
pygame.mixer.music.load('End.MP3')
# 播放状态标志位
audio_playing = False
# 用于记录身体标定点位置的空列表
body_positions = []
square_points = []

# 接着打开摄像头获取视频，并建立我们的类。
cap = cv2.VideoCapture(0)
# cap = cv2.VideoCapture("jjj.mp4")

# 初始化标定直线斜率标志位和斜率
# line_fit_done = False
# slope = None
def calculate_slope(x_coords, y_coords):#最小二乘法拟合直线
    # 使用最小二乘法计算斜率
    A = np.vstack([x_coords, np.ones(len(x_coords))]).T
    m, c = np.linalg.lstsq(A, y_coords, rcond=None)[0]
    return m

with mp_holistic.Holistic(
        model_complexity=0,#分析质量，取0、1、2，越大质量越好，帧数会降低
        min_detection_confidence=0.5,#检测置信度阈值
        min_tracking_confidence=0.5) as holistic:#追踪置信度阈值
    fig = plt.figure()
    ax = fig.add_subplot(111, projection="3d")

    while cap.isOpened():
        success, image = cap.read()
        if not success:
            print("Ignoring empty camera frame.")
            # If loading a video, use 'break' instead of 'continue'.
            continue

        start = time.time()
        image.flags.writeable = False
        # image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)#BGR图转RGB
        results = holistic.process(image)#处理三通道彩色图,返回了结点坐标,可以按需调用坐标

        # 绘制关键点并获取特定关键点的坐标
        if results.pose_landmarks:
            # 获取特定关键点的坐标
            #右手
            landmark_11 = results.pose_landmarks.landmark[11]  # 身体点12
            landmark_13 = results.pose_landmarks.landmark[13]  # 身体点14
            landmark_15 = results.pose_landmarks.landmark[15]  # 身体点16
            #左手
            landmark_12 = results.pose_landmarks.landmark[12]  # 身体点11
            landmark_14 = results.pose_landmarks.landmark[14]  # 身体点13
            landmark_16 = results.pose_landmarks.landmark[16]  # 身体点15

        # 在图像上绘制特定关键点
        # 绘制身体点12、14、16
        for landmark in [landmark_12, landmark_14, landmark_16, landmark_11, landmark_13, landmark_15]:
            cx, cy = int(landmark.x * image.shape[1]), int(landmark.y * image.shape[0])
            cv2.circle(image, (cx, cy), 5, (0, 0, 255), -1)  # 画圆点

        #双手水平，执行标定
        # 计算手提点的坐标
        x_coords = [landmark_11.x, landmark_13.x, landmark_15.x, landmark_12.x, landmark_14.x, landmark_16.x]
        y_coords = [landmark_11.y, landmark_13.y, landmark_15.y, landmark_12.y, landmark_14.y, landmark_16.y]
        # 计算斜率
        slope = calculate_slope(x_coords, y_coords)
        print("斜率:", slope)
        if -0.05< slope < 0.05 and max(y_coords) - min(y_coords) < 0.1:
            if not in_horizontal_position:
                # 如果之前不在水平位置，更新开始时间
                start_time = time.time()
                in_horizontal_position = True
            else:
                # 如果已经在水平位置，则检查时间是否超过1秒钟
                if time.time() - start_time >= 1:
                    print("双手水平")
                    print("y差值" + str(max(y_coords) - min(y_coords)))
                    start_time = time.time()  # 重置开始时间
                    # 播放音频
                    if not audio_playing:
                        pygame.mixer.music.play()
                        audio_playing = True
                        # 等待3秒
                        time.sleep(3)
                        # 停止音频
                        pygame.mixer.music.stop()
                        audio_playing = False
                        # 记录当前身体点的位置
                        body_positions.append((landmark_11.x, landmark_11.y,
                                               landmark_13.x, landmark_13.y,
                                               landmark_15.x, landmark_15.y,
                                               landmark_12.x, landmark_12.y,
                                               landmark_14.x, landmark_14.y,
                                               landmark_16.x, landmark_16.y))
                        print("标记完成")
                        # 保存到文件
                        with open('body_positions.txt', 'a') as f:
                            for pos in body_positions:
                                f.write(', '.join(map(str, pos)) + '\n')
                            body_positions = []  # 清空已保存的位置数据
        else:
            # 如果不满足条件，重置标志和开始时间
            in_horizontal_position = False
            start_time = time.time()
        # 可以根据需要添加适当的延时，以避免过多地检查条件
        time.sleep(0.1)  # 每次循环暂停0.1秒


        # 按下'q'键退出循环
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

        if results.pose_world_landmarks:
            draw3d(plt, ax, results.pose_world_landmarks)

        # 释放摄像头和关闭所有窗口
        cv2.imshow('MediaPipe Holistic', image)
cap.release()
cv2.destroyAllWindows()

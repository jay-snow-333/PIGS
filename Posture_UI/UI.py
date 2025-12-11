import sys
import subprocess
from PyQt5.QtWidgets import QApplication, QMainWindow, QVBoxLayout, QWidget, QPushButton, QLabel, QHBoxLayout

class ScriptRunner(QMainWindow):
    def __init__(self):
        super().__init__()

        self.setWindowTitle("BikeGP_Stream")
        self.setGeometry(200, 200, 400, 250)

        # 创建运行和停止按钮
        self.run_button1 = QPushButton("Run body data", self)
        self.run_button1.clicked.connect(lambda: self.run_script(1))

        self.stop_button1 = QPushButton("Stop body data", self)
        self.stop_button1.clicked.connect(lambda: self.stop_script(1))
        self.stop_button1.setEnabled(False)  # 初始状态下禁用停止按钮

        self.run_button2 = QPushButton("Run EEG data", self)
        self.run_button2.clicked.connect(lambda: self.run_script(2))

        self.stop_button2 = QPushButton("Stop EEG data2", self)
        self.stop_button2.clicked.connect(lambda: self.stop_script(2))
        self.stop_button2.setEnabled(False)  # 初始状态下禁用停止按钮

        self.status_label1 = QLabel("", self)
        self.status_label2 = QLabel("", self)

        # 布局设置
        layout = QVBoxLayout()

        # 脚本1按钮和状态标签
        hbox1 = QHBoxLayout()
        hbox1.addWidget(self.run_button1)
        hbox1.addWidget(self.stop_button1)
        layout.addLayout(hbox1)
        layout.addWidget(self.status_label1)

        # 脚本2按钮和状态标签
        hbox2 = QHBoxLayout()
        hbox2.addWidget(self.run_button2)
        hbox2.addWidget(self.stop_button2)
        layout.addLayout(hbox2)
        layout.addWidget(self.status_label2)

        container = QWidget()
        container.setLayout(layout)

        self.setCentralWidget(container)

        # 保存脚本进程对象
        self.processes = {1: None, 2: None}

    def run_script(self, script_number):
        # 替换脚本路径
        if script_number == 1:
            script_path = "2PARI.py"
        elif script_number == 2:
            script_path = "CameraCalibration.py"
        else:
            self.status_label1.setText("Invalid script number.")
            return
        script_command = f"python {script_path}"

        try:
            # 运行脚本并保存进程对象
            self.processes[script_number] = subprocess.Popen(script_command, shell=True)
            status_label = self.status_label1 if script_number == 1 else self.status_label2
            run_button = self.run_button1 if script_number == 1 else self.run_button2
            stop_button = self.stop_button1 if script_number == 1 else self.stop_button2
            if script_number ==1:
                status_label.setText(f"Body data running...")
                run_button.setEnabled(False)  # 运行后禁用运行按钮
                stop_button.setEnabled(True)  # 启用停止按钮
            if script_number ==2:
                status_label.setText(f"EEG data running...")
                run_button.setEnabled(False)  # 运行后禁用运行按钮
                stop_button.setEnabled(True)  # 启用停止按钮
        except Exception as e:
            status_label.setText(f"Error: {e}")

    def stop_script(self, script_number):
        if self.processes[script_number]:
            self.processes[script_number].terminate()  # 终止进程
            self.processes[script_number] = None
            status_label = self.status_label1 if script_number == 1 else self.status_label2
            run_button = self.run_button1 if script_number == 1 else self.run_button2
            stop_button = self.stop_button1 if script_number == 1 else self.stop_button2
            if script_number ==1:
                status_label.setText(f"Body data stoped")
                run_button.setEnabled(True)  # 运行后禁用运行按钮
                stop_button.setEnabled(False)  # 启用停止按钮
            if script_number ==2:
                status_label.setText(f"EEG data stoped")
                run_button.setEnabled(True)  # 运行后禁用运行按钮
                stop_button.setEnabled(False)  # 启用停止按钮

def main():
    app = QApplication(sys.argv)
    runner = ScriptRunner()
    runner.show()
    sys.exit(app.exec_())

if __name__ == "__main__":
    main()

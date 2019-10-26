import requests 
import time

base_url = 'http://127.0.0.1:5000/api'
rover_id = 'Elcaduck'
id_url = base_url + '/' + rover_id

pos_x = 0.0
pos_y = 0.0
pos_angle = 0.0
pos_rad = 0.0

def print_angle():
	print('angle: ' + str(pos_angle))

def update_pos():
	global pos_x
	global pos_y
	global pos_angle
	global pos_rad
	r = requests.get(url = id_url)
	data = r.json()
	print(data)
	print(data['gps_orientation'])
	pos_x = data['gps_x']
	pos_y = data['gps_y']
	pos_angle = data['gps_orientation']
	pos_rad = data['gps_orientation_rad']
	print_angle()
	
def stop():
	r = requests.get(url = id_url + '/stop')
	
def print_pos():
	print('x: ' + str(pos_x) + ' y: ' + str(pos_y))
	
def go_forward(duration: float=1.0):
	PARAMS = {'duration':duration}
	r = requests.get(url = id_url + '/forward', params = PARAMS)
	update_pos()
	
def turn(duration: float, direction: str='left'):
	PARAMS = {'duration':duration, 'direction':direction}
	r = requests.get(url = id_url + '/rotate', params = PARAMS)

def turn_to(turn_to: float):
	
	while(not (turn_to - 10 < pos_angle and pos_angle < turn_to + 10)):
		turn(0.1)
		update_pos()
		time.sleep(0.5)
	stop()
	
def turn_degrees(degrees: float):
	init_angle = pos_angle
	while(abs(init_angle - pos_angle) < degrees):
		turn(0.1)
		update_pos()
		time.sleep(0.5)
	stop()

update_pos()
print_pos()
#turn(0.01, 'right')
#turn_to(-140.0)
turn_degrees(30)
print_pos()

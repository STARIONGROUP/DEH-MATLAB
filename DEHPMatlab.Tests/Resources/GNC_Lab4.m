
%% TASK 1

% Constants 
RE = - 6370;mu = 398600;h_A = 6000;h_B = 15000;

% Initial values
 %Altitude of spacecraft A in km
 %Altitude of spacecraft B in km
r_A = RE + h_A; %Radius of orbit A in km
r_B = RE + h_B; %Radius of orbit B in km

% Orbital period in circular orbit T=2*pi*sqrt(r^3/mu)
% Orbital period for spacecraft A in s
T_A = 2*pi*sqrt(r_A^3/mu);

% Orbital period for spacecraft B in s
T_B = 2*pi*sqrt(r_B^3/mu);


%% TASK 2

%Orbit A
tspan_A = [0 T_A];
y0_A = [r_A, 0, 0, sqrt(mu/r_A)];
[tA,yA] = ode45(@(t,y) odefcn(t,y,mu), tspan_A, y0_A);

%Orbit B
tspan_B = [0 T_B];
y0_B = [r_B, 0, 0, sqrt(mu/r_B)];
[tB,yB] = ode45(@(t,y) odefcn(t,y,mu), tspan_B, y0_B);

% Plot the orbit
figure(2)
plot(yB(:,1), yB(:,3))
xlabel('x [km]')
ylabel('y [km]')
title('Orbit of spacecraft B')

%% TASK 3

% Delta-v required to enter the transfer orbit
dv1 = sqrt(mu/r_A)*(sqrt((2*r_B)/(r_A+r_B))-1);

%Delta-v required to enter orbit B from the transfer orbit
dv2 = sqrt(mu/r_B)*(1-sqrt((2*r_A)/(r_A+r_B)));

%Total delta-v required for the transfer (in km/s)
dv= dv1 + dv2;

%Orbit A
tspan_A = [0 T_A];
y0_A = [r_A, 0, 0, sqrt(mu/r_A)];
[tA,yA] = ode45(@(t,y) odefcn(t,y,mu), tspan_A, y0_A);

%Hohmann transfer
htime = pi*sqrt((r_A+r_B)^3/(8*mu));
tspan_H = [0 htime];
y0_H = [yA(end, 1), yA(end, 2), yA(end,3), yA(end,4)+dv1];
[tH,yH] = ode45(@(t,y) odefcn(t,y,mu), tspan_H, y0_H);

%Orbit B
tspan_B = [0 T_B];
y0_B = [yH(end, 1), yH(end, 2), yH(end,3), yH(end,4)-dv2];
[tB,yB] = ode45(@(t,y) odefcn(t,y,mu), tspan_B, y0_B);

% Plot the orbital transfer 
figure(3)
plot(yA(:,1), yA(:,3), yH(:,1), yH(:,3), yB(:,1), yB(:,3))
legend('Orbit A', 'Transfer', 'Orbit B')
xlabel('x [km]')
ylabel('y [km]')
title('Hohmann transfer from low to high orbit')
axis equal


%% TASK 4

% Delta-v required to enter the transfer orbit
dv1 = sqrt(mu/r_B)*(sqrt((2*r_A)/(r_A+r_B))-1);

%Delta-v required to enter orbit A from the transfer orbit
dv2 = sqrt(mu/r_A)*(1-sqrt((2*r_B)/(r_A+r_B)));

%Total delta-v required for the transfer (in km/s)
dv= dv1 + dv2;

%Orbit B
tspan_B = [0 T_B];
y0_B = [r_B, 0, 0, sqrt(mu/r_B)];
[tB,yB] = ode45(@(t,y) odefcn(t,y,mu), tspan_B, y0_B);

%Hohmann transfer
htime = pi*sqrt((r_A+r_B)^3/(8*mu));
tspan_H = [0 htime];
y0_H = [yB(end, 1), yB(end, 2), yB(end,3), yB(end,4)+dv1];
[tH,yH] = ode45(@(t,y) odefcn(t,y,mu), tspan_H, y0_H);

%Orbit A
tspan_A = [0 T_A];
y0_A = [yH(end, 1), yH(end, 2), yH(end,3), yH(end,4)-dv2];
[tA,yA] = ode45(@(t,y) odefcn(t,y,mu), tspan_A, y0_A);

% Plot the orbital transfer 
figure(3)
plot(yB(:,1), yB(:,3), yH(:,1), yH(:,3), yA(:,1), yA(:,3))
legend('Orbit B', 'Transfer', 'Orbit A')
xlabel('x [km]')
ylabel('y [km]')
title('Hohmann transfer from high to low orbit')
axis equal


%% TASK 5

% Data for guidance command generation
rm = 1737000; % m
r0 = [10000 100 6000+rm]'; % m
rf = [0 0 rm]'; % m
v0 = [144 1 -44]'; % m/s
vf = [0 0 0]'; % m/s
af = [0 0 1.62]'; % m/s^2
g  = [0 0 1.62]'; % m/s^2
tf = 160; % s

a0 =  12/tf^2*(rf-r0-v0*tf+tf^2/2*g)-6/tf^1*(vf-v0+g*tf)+af;
a1 = -48/tf^3*(rf-r0-v0*tf+tf^2/2*g)+30/tf^2*(vf-v0+g*tf)-6/tf*af;
a2 =  36/tf^4*(rf-r0-v0*tf+tf^2/2*g)-24/tf^3*(vf-v0+g*tf)+6/tf^2*af;

t = 0:1:tf;
a = a0+a1*t+a2*t.^2;
v = v0+(a0-g)*t+a1*t.^2/2+a2*t.^3/3;
r = r0+v0*t+(a0-g)*t.^2/2+a1*t.^3/6+a2*t.^4/12;

% Plot the trajectory and velocity profiles
figure(4)
sgtitle('Trajectory for x')

subplot(3,1,1)
plot(t,r(1,:))
xlabel('Time [s]')
ylabel('x [m]')

subplot(3,1,2)
plot(t,v(1,:))
xlabel('Time [s]')
ylabel('v_x [m/s]')

subplot(3,1,3)
plot(t,a(1,:))
xlabel('Time [s]')
ylabel('a_x [m/s^2]')

figure(5)
sgtitle('Trajectory for y')

subplot(3,1,1)
plot(t,r(2,:))
xlabel('Time [s]')
ylabel('y [m]')

subplot(3,1,2)
plot(t,v(2,:))
xlabel('Time [s]')
ylabel('v_y [m/s]')

subplot(3,1,3)
plot(t,a(2,:))
xlabel('Time [s]')
ylabel('a_y [m/s^2]')

figure(6)
sgtitle('Trajectory for z')

subplot(3,1,1)
plot(t,r(3,:))
xlabel('Time [s]')
ylabel('z [m]')

subplot(3,1,2)
plot(t,v(3,:))
xlabel('Time [s]')
ylabel('v_z [m/s]')

subplot(3,1,3)
plot(t,a(3,:))
xlabel('Time [s]')
ylabel('a_z [m/s^2]')


invalidUnaryPrefix = --TA;

validUnaryPrefix = --47;

array1 = [0 1 2 3]
array2 = [0,1,2,3]
array3 = [0 -2 -5 6]
array4 = [0,-2 -5,-6]
array5 = [0 -2 -5 6]'
array6 = [0,-2,-5,-6]'
array7 = [0 5 -5 -5; 0 5 -5 -5; 0 5 -5 -5]
array8 = [0 5 -5 -5; 0 5 -5 -5; 0 5 -5 -5]'
array9 = [-5 -5 -5 -6 -TA]



%% FUNCTIONS

% TASK 2
function dydt = odefcn(t,y,mu);
  r = sqrt(y(1)^2+y(3)^2);
  dydt = [y(2);
          (-mu/r^3)*y(1);
          y(4);
          (-mu/r^3)*y(3)];
end
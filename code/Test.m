fig = figure;
fig.Units = 'pixels';
fig.Position = [0 0 800 600];
fig.Resize = 'off';

% draw road surface
[X,Y] = meshgrid(0:0.5:100,-2.5:0.5:7.5);
Z = zeros(size(X));
surf(X,Y,Z);
hold on

% draw model of checker board (green square) 
[Ysgn Zsgn] = meshgrid(-0.5: 0.5, 2 : 3);

% distance on x-axis 15 m as for the virtual world
Xsgn = ones(size(Zsgn))*15;
surf(Xsgn, Ysgn, Zsgn);

p = gca;
set(gca, 'CameraViewAngle', 47)
daspect(p, [1 1 1])

camproj('perspective')
campos([0,0,2.5])
camtarget([50,0,2.5])

ylim([-2.5 7.5])
zlim([0 5])

clear;
clc;

% https://stackoverflow.com/questions/29632430/quiver3-arrow-color-corresponding-to-magnitude

% Force hardware OpenGL rendering?
opengl hardware
set(0, 'DefaultFigureRenderer', 'OpenGL');
    
data_directory = '../data2';

% Read images
image_height = rgb2gray(imread(strcat(data_directory, '/heightmap.jpg')));

x_amp_texture = imread(strcat(data_directory, '/x_amp.jpg'));
y_amp_texture = imread(strcat(data_directory, '/y_amp.jpg'));
z_amp_texture = imread(strcat(data_directory, '/z_amp.jpg'));

image_ampl_x = rgb2gray(x_amp_texture);
image_ampl_y = rgb2gray(y_amp_texture);
image_ampl_z = rgb2gray(z_amp_texture);

image_phase_x = rgb2gray(imread(strcat(data_directory, '/x_phase.jpg')));
image_phase_y = rgb2gray(imread(strcat(data_directory, '/y_phase.jpg')));
image_phase_z = rgb2gray(imread(strcat(data_directory, '/z_phase.jpg')));

% Image sizing parameters
sample_count = 200;
imageStartX = 150; % x offset
imageEndX = 933;
imageStartY = 36; % y offset
imageEndY = 818;

if strcmp(data_directory, '../data2')
    imageStartX = 1;
    imageStartY = 1;
    imageEndX = 782;
    imageEndY = 782;
end

% Drawing matrices, variables
draw_height = zeros(sample_count, sample_count);
draw_amp_x = zeros(sample_count, sample_count);
draw_amp_y = zeros(sample_count, sample_count);
draw_amp_z = zeros(sample_count, sample_count);

draw_amp_z_offset = zeros(sample_count, sample_count);

draw_amp_vec_x = NaN(sample_count, sample_count);
draw_amp_vec_y = NaN(sample_count, sample_count);
draw_amp_vec_z = NaN(sample_count, sample_count);

imageStepX = (imageEndX - imageStartX) / sample_count;
imageStepY = (imageEndX - imageStartX) / sample_count;
imageCounterX = 1;
imageCounterY = 1;

vector_frequency = 5;
vector_scale = 1;
xy_scale = 10;

for x = imageStartX : imageStepX : imageEndX
    imageCounterY=1;
 
    for y = imageStartY : imageStepY : imageEndY
        newX = int32(x);
        newY = int32(y);
        
        if(imageCounterY > sample_count) 
            break;
        end
        
        if(imageCounterX > sample_count) 
            break;
        end
        
        height_val = double(image_height(newY, newX)) / 255;
        draw_height(imageCounterX, imageCounterY) = height_val;
        
        % Amplitudes and stuff   
        amp_x_val = func_readAmp(image_phase_x, image_ampl_x, newX, newY);
        amp_y_val = func_readAmp(image_phase_y, image_ampl_y, newX, newY);
        amp_z_val = func_readAmp(image_phase_z, image_ampl_z, newX, newY);

        draw_amp_x(imageCounterX, imageCounterY) = amp_x_val;
        draw_amp_y(imageCounterX, imageCounterY) = amp_y_val;
        draw_amp_z(imageCounterX, imageCounterY) = amp_z_val;
        
        % Vector fields
        if mod(imageCounterX, vector_frequency) == 0 && mod(imageCounterY, vector_frequency) == 0
            draw_amp_vec_x(imageCounterX, imageCounterY) = amp_x_val * vector_scale * xy_scale;
            draw_amp_vec_y(imageCounterX, imageCounterY) = amp_y_val * vector_scale * xy_scale;
            draw_amp_vec_z(imageCounterX, imageCounterY) = amp_z_val * vector_scale;
        end
        
        % Amp + offset
        draw_amp_z_offset(imageCounterX, imageCounterY) = draw_amp_z(imageCounterX, imageCounterY) + height_val;
        
        
        imageCounterY = imageCounterY + 1;
    end
    
   imageCounterX = imageCounterX + 1;
end

% Set up plot camera stuff
F = figure();
func_OGL(F);
zlim([-0.5 2]);
xlim([0 200]);
ylim([0 200]);
daspect([1 1 0.1]);
callback_SetColormap(0, 0, summer);
hold on;
camproj('perspective');

global surface_amp_z;
surface_amp_z = surf(draw_amp_x);
surface_amp_z.EdgeAlpha = 0.5;
surface_amp_z.FaceAlpha = 0.7;
surface_amp_z.FaceColor = 'r';

global surface_amp_z_offset;
surface_amp_z_offset = surf(draw_amp_z_offset);
surface_amp_z_offset.EdgeAlpha = surface_amp_z.EdgeAlpha;
surface_amp_z_offset.FaceAlpha = surface_amp_z.FaceAlpha;
surface_amp_z_offset.FaceColor = surface_amp_z.FaceColor;
surface_amp_z_offset.Visible = 'off';

% SURFACE NORMAL THINGIES
global surface_vectors;
surface_vectors = quiver3(draw_height, draw_amp_vec_x, draw_amp_vec_y, draw_amp_vec_z);
surface_vectors.Color = 'r';
surface_vectors.LineWidth = 1;
surface_vectors.ShowArrowHead = 'off';
surface_vectors.AutoScale = 'off';

% X, Y, Z amplitude textures on the heightmap as separate surfaces.
% Buttons toggle between them
global surface_heightmap_x;
surface_heightmap_x = warp(draw_height, flip(imrotate(x_amp_texture, -90), 2));

global surface_heightmap_y;
surface_heightmap_y = warp(draw_height, flip(imrotate(y_amp_texture, -90), 2));

global surface_heightmap_z;
surface_heightmap_z = warp(draw_height, flip(imrotate(z_amp_texture, -90), 2));

callback_SetNoTexturedHeightmap();
callback_SetTexturedHeightmap(0, 0, surface_heightmap_z);

% GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI GUI
createButton(F, 'Amp. Z', 1, {@callback_AmpZ});
createButton(F, 'Amp. Z + Height', 2, {@callback_AmpZHeight});
createButton(F, 'No Amp.', 3, {@callback_NoAmp});
createButton(F, 'Toggle Vectors', 4, {@callback_ToggleVis, surface_vectors});

createButton(F, 'X amp overlay', 6, {@callback_SetTexturedHeightmap, surface_heightmap_x});
createButton(F, 'Y amp overlay', 7, {@callback_SetTexturedHeightmap, surface_heightmap_y});
createButton(F, 'Z amp overlay', 8, {@callback_SetTexturedHeightmap, surface_heightmap_z});
createButton(F, 'No amp overlay', 9, {@callback_SetNoTexturedHeightmap});

createButton(F, 'Summer', 11, {@callback_SetColormap, summer});
createButton(F, 'Winter', 12, {@callback_SetColormap, winter});

callback_NoAmp();

function [amp] = func_readAmp(image_phase, image_amp, x, y)
    phase_val = double(image_phase(y, x)) / 255;
    amp_val = double(image_amp(y, x)) / 255;
    
    if phase_val > 0.5
        amp_val = -amp_val;
    end
    
    amp = amp_val;
end

function [] = func_OGL(fig)
    set(fig, 'Renderer', 'OpenGL');
    set(fig, 'RendererMode', 'manual');
end

function [] = callback_SetTexturedHeightmap(varargin)
    textured_heightmap = varargin{3};
    callback_SetNoTexturedHeightmap();
    
    % Turn on enabled one
    surface_heightmap = textured_heightmap;
    surface_heightmap.Visible = 'on';
    surface_heightmap.FaceAlpha = 0.9;
    surface_heightmap.EdgeColor = 'none';
end

function [] = callback_SetNoTexturedHeightmap(varargin)
    global surface_heightmap_x;
    global surface_heightmap_y;
    global surface_heightmap_z;
    
    % Turn off all heightmaps
    surface_heightmap_x.Visible = 'off';
    surface_heightmap_y.Visible = 'off';
    surface_heightmap_z.Visible = 'off';
end

function [] = callback_AmpZ(varargin)
    global surface_amp_z;
    global surface_amp_z_offset;
    
    surface_amp_z.Visible = 'on';
    surface_amp_z_offset.Visible = 'off';
end

function [] = callback_AmpZHeight(varargin)
    global surface_amp_z;
    global surface_amp_z_offset;
    
    surface_amp_z.Visible = 'off';
    surface_amp_z_offset.Visible = 'on';
end

function [] = callback_NoAmp(varargin)
    global surface_amp_z;
    global surface_amp_z_offset;
    
    surface_amp_z.Visible = 'off';
    surface_amp_z_offset.Visible = 'off';
end

function [] = callback_SetColormap(varargin)
    colormap(varargin{3});
end

function [] = callback_ToggleVis(varargin)
    structure = varargin{3};
    
    if strcmp(structure.Visible, 'on')
        structure.Visible = 'off';
    else
        structure.Visible = 'on';
    end
end

function [btn] = createButton(prnt, txt, idx, cback)
    btn_height = 0.035;

    btn = uicontrol();
    btn.Parent = prnt;
    btn.Style = 'pushbutton';
    btn.String = txt;
    btn.Units = 'normalized';
    btn.Position = [0.0, 1 - idx * btn_height, 0.12, btn_height];
    btn.Callback = cback;
end

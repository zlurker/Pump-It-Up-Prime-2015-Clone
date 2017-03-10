local t = Def.ActorFrame{   
	LoadActor("1")..{
		InitCommand=cmd(x,SCREEN_CENTER_X;y,SCREEN_CENTER_Y+188;diffusealpha,1;zoom,0.3;linear,999);
	};
};
return t;
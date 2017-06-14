return Def.ActorFrame{   
	LoadActor("X")..{
		InitCommand=cmd(x,SCREEN_CENTER_X;y,SCREEN_CENTER_Y+175;zoom,0.35);
		OnCommand=cmd(diffusealpha,0.35;linear,240);
	}
};

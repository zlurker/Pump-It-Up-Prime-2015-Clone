local t = Def.ActorFrame{   
	LoadActor("XKS")..{
		InitCommand=cmd(xy,0,0;diffusealpha,1;);
		OnCommand=cmd(linear,0;zoom,1;linear,2;zoom,1.2;linear,2;zoom,1;queuecommand,"On");
	};
	LoadActor("XKS")..{
		InitCommand=cmd(xy,0,0;diffusealpha,1;);
		OnCommand=cmd(linear,0;zoom,1;diffusealpha,1;linear,2;zoom,1.4;diffusealpha,0;linear,2;zoom,1;queuecommand,"On");
	};
};
return t;
function makeScene ()

	local scene = MOAILayer2D.new ()

	local viewport = MOAIViewport.new ()
	viewport:setRect ( -64, -64, 64, 64 )
	viewport:setScale ( 128, 128 )
	scene:setViewport ( viewport )

	local tileset = MOAITileLibrary.new ()
	tileset:setTexture ( "numbers.png" )
	tileset:setSize ( 8, 8 )

	local grid = MOAIGrid.new ()
	grid:setSize ( 4, 4, 32, 32 )

	grid:setRow ( 0, 	0x00, 0x01, 0x02, 0x03 )
	grid:setRow ( 1, 	0x08, 0x09, 0x0a, 0x0b )
	grid:setRow ( 2, 	0x10, 0x11, 0x12, 0x13 )
	grid:setRow ( 3, 	0x18, 0x19, 0x1a, 0x1b )

	local tilemap = MOAITilemap.new ()
	tilemap:setGfxSource ( tileset )
	tilemap:setGrid ( grid )
	tilemap:setRepeat ( true )
	scene:insertPrim ( tilemap )

	tilemap:moveRot ( 360, 2 )
	
	return scene;
end

function makeCathead ()

	local sprite = MOAISprite2D.new ()

	local texture = MOAITexture.new ()
	texture:load ( "cathead.png" )
	texture:setRect ( -128, -128, 128, 128 )

	sprite:setGfxSource ( texture )

	sprite:moveLoc ( -64, -64, 2 )
	
	return sprite
end

scene = MOAILayer2D.new ()
MOAISim.pushRenderPass ( scene )

viewport = MOAIViewport.new ()
viewport:setSize ( 256, 256 )
viewport:setScale ( 256, 256 )
scene:setViewport ( viewport )

cathead = makeCathead ()
scene:insertPrim ( cathead )

innerScene = makeScene ()
innerScene:setParent ( cathead );
scene:insertPrim ( innerScene )

MOAISim.openWindow ( "tilemap", 256, 256 )
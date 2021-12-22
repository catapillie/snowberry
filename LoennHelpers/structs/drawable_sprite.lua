local utils = require("utils")
local loennPluginLoader = require("#Snowberry.LoennPluginLoader")

local drawableSpriteStruct = {}

local drawableSpriteMt = {}
drawableSpriteMt.__index = {}

function drawableSpriteMt.__index:setJustification(justificationX, justificationY)
    self.justificationX = justificationX
    self.justificationY = justificationY

    return self
end

function drawableSpriteMt.__index:setPosition(x, y)
    self.x = x
    self.y = y

    return self
end

function drawableSpriteMt.__index:addPosition(x, y)
    self.x = x + self.x
    self.y = y + self.y

    return self
end

function drawableSpriteMt.__index:setScale(scaleX, scaleY)
    self.scaleX = scaleX
    self.scaleY = scaleY

    return self
end

function drawableSpriteMt.__index:setOffset(offsetX, offsetY)
    self.offsetX = offsetX
    self.offsetY = offsetY

    return self
end

local function setColor(target, color)
    local tableColor = utils.getColor(color)

    if tableColor then
        target.color = tableColor
    end

    return not (tableColor == nil)
end

function drawableSpriteMt.__index:setColor(color)
    return setColor(self, color)
end

-- TODO - Verify that scales are correct
function drawableSpriteMt.__index:getRectangleRaw()
    local x = self.x
    local y = self.y

    local width = self.meta.width
    local height = self.meta.height

    local realWidth = self.meta.realWidth
    local realHeight = self.meta.realHeight

    local offsetX = self.offsetX or self.meta.offsetX
    local offsetY = self.offsetY or self.meta.offsetY

    local justificationX = self.justificationX
    local justificationY = self.justificationY

    local rotation = self.rotation

    local scaleX = self.scaleX
    local scaleY = self.scaleY

    local drawX = math.floor(x - (realWidth * justificationX + offsetX) * scaleX)
    local drawY = math.floor(y - (realHeight * justificationY + offsetY) * scaleY)

    drawX = drawX + (scaleX < 0 and width * scaleX or 0)
    drawY = drawX + (scaleY < 0 and height * scaleY or 0)

    local drawWidth = width * math.abs(scaleX)
    local drawHeight = height * math.abs(scaleY)

    --[[if rotation and rotation ~= 0 then
        -- Shorthand for each corner
        -- Remove x and y before rotation, otherwise we rotate around the wrong origin
        local tlx, tly = drawX - x, drawY - y
        local trx, try = drawX - x + drawWidth, drawY - y
        local blx, bly = drawX - x, drawY - y + drawHeight
        local brx, bry = drawX - x + drawWidth, drawY - y + drawHeight

        -- Apply rotation
        tlx, tly = utils.rotate(tlx, tly, rotation)
        trx, try = utils.rotate(trx, try, rotation)
        blx, bly = utils.rotate(blx, bly, rotation)
        brx, bry = utils.rotate(brx, bry, rotation)

        -- Find the best point for top left and bottom right
        local bestTlx, bestTly = math.min(tlx, trx, blx, brx), math.min(tly, try, bly, bry)
        local bestBrx, bestBry = math.max(tlx, trx, blx, brx), math.max(tly, try, bly, bry)

        drawX, drawY = utils.round(x + bestTlx), utils.round(y + bestTly)
        drawWidth, drawHeight = utils.round(bestBrx - bestTlx), utils.round(bestBry - bestTly)
    end]]--

    return drawX, drawY, drawWidth, drawHeight
end

function drawableSpriteMt.__index:getRectangle()
    return utils.rectangle(self:getRectangleRaw())
end

function drawableSpriteStruct.fromMeta(meta, data)
    data = data or {}

    local drawableSprite = {
        _type = "drawableSprite"
    }

    drawableSprite.x = data.x or 0
    drawableSprite.y = data.y or 0

    drawableSprite.justificationX = data.jx or data.justificationX or 0.5
    drawableSprite.justificationY = data.jy or data.justificationY or 0.5

    drawableSprite.scaleX = data.sx or data.scaleX or 1
    drawableSprite.scaleY = data.sy or data.scaleY or 1

    drawableSprite.rotation = data.r or data.rotation or 0

    drawableSprite.depth = data.depth

    drawableSprite.meta = meta

    if data.color then
        setColor(drawableSprite, data.color)
    end
    
    return setmetatable(drawableSprite, drawableSpriteMt)
end

function drawableSpriteStruct.fromTexture(texture, data)
    local atlas = data and data.atlas or "Gameplay"
    local spriteMeta = loennPluginLoader.LuaGetImage(texture, atlas)

    if spriteMeta then
        return drawableSpriteStruct.fromMeta(spriteMeta, data)
    end
end

return drawableSpriteStruct
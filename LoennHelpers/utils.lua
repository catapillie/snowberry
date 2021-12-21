local xnaColors = require("consts.xna_colors")
local rectangles = require("structs.rectangle")

local utils = {}

function utils.rectangle(x, y, width, height)
    return rectangles.create(x, y, width, height)
end

function utils.point(x, y)
    return rectangles.create(x, y, 1, 1)
end

function utils.parseHexColor(color)
    color = color:match("^#?([0-9a-fA-F]+)$")

    if color then
        if #color == 6 then
            local number = tonumber(color, 16)
            local r, g, b = math.floor(number / 256^2) % 256, math.floor(number / 256) % 256, math.floor(number) % 256

            return true, r / 255, g / 255, b / 255

        elseif #color == 8 then
            local number = tonumber(color, 16)
            local r, g, b = math.floor(number / 256^3) % 256, math.floor(number / 256^2) % 256, math.floor(number / 256) % 256
            local a = math.floor(number) % 256

            return true, r / 255, g / 255, b / 255, a / 255
        end
    end

    return false, 0, 0, 0
end

function utils.getXNAColor(name)
    name = name:lower()
    
    for cName, c in pairs(xnaColors) do
        if cName:lower() == name then
            return c, cName
        end
    end
    
    return false, false
end

function utils.getColor(color)
    if type(color) == "string" then
        local xnaColor = utils.getXNAColor(color)
        if xnaColor then
            return xnaColor
        end
        
        local success, r, g, b = utils.parseHexColor(color)
        if success then
            return {r, g, b}
        end
        return success
        
    elseif type(color) == "table" and (#color == 3 or #color == 4) then
        return color
    end
    
    return {1, 1, 1}
end

return utils
local utils = {}

table.insert(package.searchers, function(name) return snowberryRequires:get(name) and function() return snowberryRequires:get(name) end or "Not a Snowberry default type." end)

function utils.rectangle(x, y, width, height)
    return require("structs.rectangle").create(x, y, width, height)
end

function utils.point(x, y)
    return require("structs.rectangle").create(x, y, 1, 1)
end

return utils
local modHandler = {}

function modHandler.simpleSplit(input, sep)
    local ret = {}
    for p in string.gmatch(input, "([^"..sep.."]+)") do
        table.insert(ret, p)
    end
    return ret
end

function modHandler.requireFromPlugin(lib, modName)
    local libPrefix

    if modName then
        -- TODO: find mod by name
    else
        local info = debug.getinfo(2)
        local source = info.source
        local parts = modHandler.simpleSplit(source, "/")

        libPrefix = parts[1]
    end

    if lib and libPrefix then
        local requireName = libPrefix .. "/" .. table.concat(modHandler.simpleSplit(lib, "."), "/")
        local result = require(requireName)

        return result
    else
        -- TODO: warn
    end
end

return modHandler
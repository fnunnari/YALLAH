def rreload(module):
    """
    Recursive reload of the specified module and (recursively) the used ones.
    Mandatory! Every submodule must have an __init__.py file
    Usage:
        import mymodule
        rreload(mymodule)

    :param module: the module to load (the module itself, not a string)
    :return: nothing
    """

    import os.path
    import sys

    def rreload_deep_scan(module, rootpath, mdict={}):

        from types import ModuleType
        from importlib import reload

        if module not in mdict:
            # modules reloaded from this module
            mdict[module] = []
        # print("RReloading " + str(module))
        reload(module)
        for attribute_name in dir(module):
            attribute = getattr(module, attribute_name)
            # print ("for attr "+attribute_name)
            if type(attribute) is ModuleType:
                # print ("typeok")
                if attribute not in mdict[module]:
                    # print ("not int mdict")
                    if attribute.__name__ not in sys.builtin_module_names:
                        # print ("not a builtin")
                        # If the submodule is a python file, it will have a __file__ attribute
                        if not hasattr(attribute, '__file__'):
                            raise BaseException("Could not find attribute __file__ for module '" + str(
                                attribute) + "'. Maybe a missing __init__.py file?")

                        attribute_path = os.path.dirname(attribute.__file__)

                        if attribute_path.startswith(rootpath):
                            # print ("in path")
                            mdict[module].append(attribute)
                            rreload_deep_scan(attribute, rootpath, mdict)

    rreload_deep_scan(module, rootpath=os.path.dirname(module.__file__))


import yallah
rreload(yallah)

# Execute and THEN PRESS F8 to reload the module


import sys
import os

def redirect_output():
    try:
        # Check if we're in windowed mode (no console)
        if hasattr(sys, 'frozen'):
            # Redirect stdout and stderr to NUL
            sys.stdout = open('NUL', 'w')
            sys.stderr = open('NUL', 'w')
    except Exception:
        pass

redirect_output()
{
  # Today, we just use this for nix environments to create a dev shell.
  # In the future, we might use this to build the actual application.

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-23.11";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, flake-utils, nixpkgs }: {
  } // flake-utils.lib.eachDefaultSystem (system: let 
    pkgs = import nixpkgs {
      inherit system;
      config.allowUnfree = true;
    };
  in {
    devShell = pkgs.mkShell {
      packages = with pkgs; [
        nodejs
        jetbrains.rider
        bat
        (with dotnetCorePackages; combinePackages [
          sdk_8_0
          runtime_6_0
        ])
      ];
      
      shellHook = ''
        export VIOLET_IN_DEV_SHELL=1
        export VIOLET_RIDER_PATH=${pkgs.jetbrains.rider}
        export VIOLET_DOTNET_SDK_PATH=${pkgs.dotnetCorePackages.sdk_8_0}
      '';
    };
  });
}

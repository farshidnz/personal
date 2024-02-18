import { getEnv, getResourceName } from "@cashrewards/cdk-lib";
import { App, StackProps } from "aws-cdk-lib";
import { config } from "dotenv";
import { existsSync } from "fs";
import * as path from "path";
import { Cr3ApiStack } from "../lib/cr3api-stack";

export class Cr3ApiApp extends App {
  protected stackProps: StackProps;
  public cr3ApiStack: Cr3ApiStack;
  constructor() {
    super();
    this.stackProps = {
      env: {
        account: getEnv("AWS_ACCOUNT_ID"),
        region: getEnv("AWS_REGION"),
      },
    };
    this.cr3ApiStack = new Cr3ApiStack(
      this,
      getResourceName(getEnv("PROJECT_NAME")),
      this.stackProps
    );
  }
}

(async () => {
  const envFile = path.resolve(process.cwd(), "../.env");
  if (existsSync(envFile)) {
    config({
      path: envFile,
    });
  } else {
    config();
  }

  return new Cr3ApiApp();
})();

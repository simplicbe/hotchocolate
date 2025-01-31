import { GatsbyLinkProps, Link as GatsbyLink } from "gatsby";
import { OutboundLink } from "gatsby-plugin-google-analytics";
import React, { FC } from "react";

export const Link: FC<
  Pick<GatsbyLinkProps<unknown>, "to" | "onClick"> & { prefetch?: false }
> = ({ to, prefetch = true, ...rest }) => {
  const internal = /^\/(?!\/)/.test(to);

  return internal ? (
    prefetch ? (
      <GatsbyLink to={to} {...rest} />
    ) : (
      <a href={to} {...rest} />
    )
  ) : (
    <OutboundLink
      href={to}
      target="_blank"
      rel="noopener noreferrer"
      {...rest}
    />
  );
};

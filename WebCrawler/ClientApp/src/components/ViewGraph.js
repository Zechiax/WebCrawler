import React, { Component } from "react";
import { useLocation } from "react-router";

export const ViewGraph = (props) => {
  const location = useLocation();
  return <ViewGraphInternal ids={location.state.ids} {...props} />;
};

class ViewGraphInternal extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    return <h1>{this.props.ids}</h1>;
  }
}

import React, { Component } from "react";

export class NodeInfo extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    return (
      <ul>
        {this.props.nodeInfos.map((info) => {
          <li>
            {info.label}: {info.value}
          </li>;
        })}
        ;
      </ul>
    );
  }
}
